using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.Web.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("mjerenja")]
public class MjerenjeController : BaseController
{
    private readonly MjerenjeRepository _mjerenjeRepository;
    private readonly KorisnikRepository _korisnikRepository;

    public MjerenjeController(MjerenjeRepository mjerenjeRepository, KorisnikRepository korisnikRepository,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _mjerenjeRepository = mjerenjeRepository;
        _korisnikRepository = korisnikRepository;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "MjerenjaIndex")]
    public IActionResult Index(int? userId, string sortBy = "datumDesc")
    {
        // 1. Izvlači sve mjerenja sa Korisnik navigacijom
        var allMjerenja = _mjerenjeRepository.GetAll().ToList();

        // 2. Filtrira po korisniku ako je zadano
        if (userId.HasValue)
        {
            allMjerenja = allMjerenja.Where(m => m.KorisnikId == userId.Value).ToList();
        }

        // 3. Sortira po datumu (novije prvo) - default
        var sortedMjerenja = allMjerenja
            .OrderByDescending(m => m.DatumMjerenja)
            .ToList();

        // 4. Kalkulira trendove za svako mjerenje
        var mjerenjaWithTrends = sortedMjerenja.Select((mjerenje, index) =>
        {
            var userMjerenja = sortedMjerenja
                .Where(m => m.KorisnikId == mjerenje.KorisnikId)
                .OrderByDescending(m => m.DatumMjerenja)
                .ToList();

            var currentIndex = userMjerenja.IndexOf(mjerenje);
            
            double? previousWeight = null;
            double? weightChange = null;
            double? percentageChange = null;
            string trendDirection = "neutral";
            bool isFirst = currentIndex == 0 && userMjerenja.Count == 1;

            // Ako postoji prethodno mjerenje
            if (currentIndex < userMjerenja.Count - 1)
            {
                var previousMjerenje = userMjerenja[currentIndex + 1];
                previousWeight = previousMjerenje.Tezina;
                weightChange = mjerenje.Tezina - previousMjerenje.Tezina;
                percentageChange = (weightChange.Value / previousMjerenje.Tezina) * 100;

                trendDirection = weightChange > 0 ? "up" : (weightChange < 0 ? "down" : "neutral");
            }

            return new MjerenjeWithTrend
            {
                Mjerenje = mjerenje,
                PreviousWeight = previousWeight,
                WeightChange = weightChange,
                PercentageChange = percentageChange,
                TrendDirection = trendDirection,
                IsFirstMeasurement = isFirst
            };
        }).ToList();

        // 5. Sortira po korisniku pri prikazu (za vizualni groping ako potrebno)
        var groupedMjerenja = mjerenjaWithTrends
            .GroupBy(m => m.Mjerenje!.KorisnikId)
            .SelectMany(g => g)
            .ToList();

        // 6. Gradi AvailableUsers kao KorisnikOption s brojem mjerenja
        var availableUsers = _korisnikRepository.GetAll()
            .Select(k => new MjerenjeIndexViewModel.MjerenjeKorisnikOption
            {
                Id = k.Id,
                FullName = $"{k.Ime} {k.Prezime}".Trim(),
                MjerenjaCount = k.Mjerenja.Count
            })
            .OrderBy(k => k.FullName)
            .ToList();

        // 7. Prosljeđuje ViewModel
        var filterName = userId.HasValue 
            ? availableUsers.FirstOrDefault(k => k.Id == userId.Value)?.FullName 
            : null;

        var viewModel = new MjerenjeIndexViewModel
        {
            Mjerenja = groupedMjerenja,
            AvailableUsers = availableUsers,
            SelectedUserId = userId,
            CurrentFilterName = filterName,
            SortBy = sortBy,
            TotalMjerenjaCount = allMjerenja.Count
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    [HttpGet("filter")]
    public IActionResult Filter(int? userId)
    {
        var allMjerenja = _mjerenjeRepository.GetAll().ToList();
        if (userId.HasValue)
            allMjerenja = allMjerenja.Where(m => m.KorisnikId == userId.Value).ToList();

        var sortedMjerenja = allMjerenja.OrderByDescending(m => m.DatumMjerenja).ToList();
        return Json(new { count = sortedMjerenja.Count, mjerenja = sortedMjerenja.Select(m => new { id = m.Id, naziv = $"{m.DatumMjerenja:dd.MM.yyyy.} — {m.Tezina} kg — {(m.Korisnik != null ? $"{m.Korisnik.Ime} {m.Korisnik.Prezime}" : "Nepoznat")}" }) });
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "MjerenjeDetails")]
    public IActionResult Details(int id)
    {
        var mjerenje = _mjerenjeRepository.GetById(id);

        if (mjerenje is null)
        {
            return NotFound();
        }

        // Pronađi korisnika
        var korisnik = _korisnikRepository.GetById(mjerenje.KorisnikId);
        if (korisnik is null)
        {
            return NotFound();
        }

        // Sve mjerenje ovog korisnika sortirane po datumu
        var userMjerenja = korisnik.Mjerenja
            .OrderByDescending(m => m.DatumMjerenja)
            .ToList();

        // Pronađi index trenutnog mjerenja
        var currentIndex = userMjerenja.FindIndex(m => m.Id == id);
        
        // Pronađi prethodno mjerenje
        var previousMjerenje = currentIndex < userMjerenja.Count - 1 
            ? userMjerenja[currentIndex + 1] 
            : null;

        // Kalkuliraj trend
        double? weightChange = null;
        double? percentageChange = null;
        string trendDirection = "neutral";
        bool isFirstMeasurement = currentIndex == userMjerenja.Count - 1;

        if (previousMjerenje != null)
        {
            weightChange = mjerenje.Tezina - previousMjerenje.Tezina;
            percentageChange = (weightChange.Value / previousMjerenje.Tezina) * 100;
            trendDirection = weightChange > 0 ? "up" : (weightChange < 0 ? "down" : "neutral");
        }

        // Pronađi relevantne ciljeve (Mrsavljenje ili DobivanjeMase)
        var relevantGoals = korisnik.Ciljevi
            .Where(c => c.Tip == TipCilja.Mrsavljenje || c.Tip == TipCilja.DobivanjeMase)
            .ToList();

        // Statistika - samo mjerenja iz ove godine
        var thisYearMjerenja = userMjerenja
            .Where(m => m.DatumMjerenja.Year == DateTime.Now.Year)
            .ToList();

        var lowestWeight = thisYearMjerenja.Count > 0 
            ? thisYearMjerenja.Min(m => m.Tezina) 
            : (double?)null;

        var highestWeight = thisYearMjerenja.Count > 0 
            ? thisYearMjerenja.Max(m => m.Tezina) 
            : (double?)null;

        var averageWeight = thisYearMjerenja.Count > 0 
            ? thisYearMjerenja.Average(m => m.Tezina) 
            : (double?)null;

        // Kreiraj ViewModel
        var viewModel = new MjerenjeDetailsViewModel
        {
            Mjerenje = mjerenje,
            PreviousMjerenje = previousMjerenje,
            WeightChange = weightChange,
            PercentageChange = percentageChange,
            TrendDirection = trendDirection,
            IsFirstMeasurement = isFirstMeasurement,
            RelevantGoals = relevantGoals,
            TotalMeasurementsThisYear = thisYearMjerenja.Count,
            LowestWeight = lowestWeight,
            HighestWeight = highestWeight,
            AverageWeight = averageWeight,
            AllUserMjerenja = userMjerenja.OrderBy(m => m.DatumMjerenja).ToList()
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("novo", Name = "MjerenjeCreate")]
    public IActionResult Create()
    {
        var model = new MjerenjeCreateViewModel
        {
            DatumMjerenja = DateTime.Today
        };

        PopulateKorisnikOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("novo")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MjerenjeCreateViewModel model)
    {
        // Provjera je li korisnik postoji
        Korisnik? korisnik = null;
        if (!model.KorisnikId.HasValue)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Korisnik je obavezan.");
        }
        else
        {
            korisnik = _korisnikRepository.GetById(model.KorisnikId.Value);
            if (korisnik is null)
                ModelState.AddModelError(nameof(model.KorisnikId), "Odabrani korisnik ne postoji.");
        }

        // Provjera je li datum nakon datuma rođenja korisnika
        if (korisnik != null && model.DatumMjerenja < korisnik.DatumRodjenja)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne smije biti prije datuma rođenja.");
        }

        // Provjera je li datum u budućnosti
        if (model.DatumMjerenja > DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne smije biti u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            PopulateKorisnikOptions(model);
            return View(model);
        }

        var mjerenje = new Mjerenje
        {
            KorisnikId = model.KorisnikId!.Value,
            DatumMjerenja = model.DatumMjerenja,
            Tezina = model.Tezina,
            PostotakMasti = model.PostotakMasti,
            OpsegStruka = model.OpsegStruka,
            Korisnik = korisnik!
        };

        _mjerenjeRepository.Add(mjerenje);

        return RedirectToRoute("MjerenjeDetails", new { id = mjerenje.Id });
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "MjerenjeEdit")]
    public IActionResult Edit(int id)
    {
        var mjerenje = _mjerenjeRepository.GetById(id);
        if (mjerenje is null)
            return NotFound();

        var model = new MjerenjeCreateViewModel
        {
            Id = mjerenje.Id,
            KorisnikId = mjerenje.KorisnikId,
            DatumMjerenja = mjerenje.DatumMjerenja,
            Tezina = mjerenje.Tezina,
            PostotakMasti = mjerenje.PostotakMasti,
            OpsegStruka = mjerenje.OpsegStruka,
            KorisnikNaziv = mjerenje.Korisnik != null ? $"{mjerenje.Korisnik.Ime} {mjerenje.Korisnik.Prezime}" : string.Empty
        };

        PopulateKorisnikOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "MjerenjeEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, MjerenjeCreateViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var mjerenje = _mjerenjeRepository.GetById(id);
        if (mjerenje is null)
            return NotFound();

        // Provjera je li korisnik postoji
        Korisnik? korisnikEdit = null;
        if (!model.KorisnikId.HasValue)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Korisnik je obavezan.");
        }
        else
        {
            korisnikEdit = _korisnikRepository.GetById(model.KorisnikId.Value);
            if (korisnikEdit is null)
                ModelState.AddModelError(nameof(model.KorisnikId), "Odabrani korisnik ne postoji.");
        }

        // Provjera je li datum nakon datuma rođenja korisnika
        if (korisnikEdit != null && model.DatumMjerenja < korisnikEdit.DatumRodjenja)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne smije biti prije datuma rođenja.");
        }

        // Provjera je li datum u budućnosti
        if (model.DatumMjerenja > DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne smije biti u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            PopulateKorisnikOptions(model);
            return View(model);
        }

        mjerenje.KorisnikId = model.KorisnikId!.Value;
        mjerenje.DatumMjerenja = model.DatumMjerenja;
        mjerenje.Tezina = model.Tezina;
        mjerenje.PostotakMasti = model.PostotakMasti;
        mjerenje.OpsegStruka = model.OpsegStruka;

        _mjerenjeRepository.Update();

        return RedirectToRoute("MjerenjeDetails", new { id = mjerenje.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "MjerenjeDelete")]
    public IActionResult Delete(int id)
    {
        var mjerenje = _mjerenjeRepository.GetById(id);
        if (mjerenje is null)
            return NotFound();

        return View(mjerenje);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "MjerenjeDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _mjerenjeRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateKorisnikOptions(MjerenjeCreateViewModel model)
    {
        model.AvailableKorisnici = _korisnikRepository.GetAll()
            .OrderBy(k => k.Prezime)
            .ThenBy(k => k.Ime)
            .Select(k => new MjerenjeKorisnikOption
            {
                Id = k.Id,
                FullName = $"{k.Ime} {k.Prezime}"
            })
            .ToList();
    }
}
