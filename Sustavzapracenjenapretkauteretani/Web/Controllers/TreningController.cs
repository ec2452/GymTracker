using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("treninzi")]
public class TreningController : BaseController
{
    private readonly TreningRepository _treningRepository;
    private readonly KorisnikRepository _korisnikRepository;

    public TreningController(TreningRepository treningRepository, KorisnikRepository korisnikRepository,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _treningRepository = treningRepository;
        _korisnikRepository = korisnikRepository;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "TreninziIndex")]
    public IActionResult Index(int? userId, VrstaTreninga? vrsta, int period = 0, string sort = "date_desc")
    {
        var sviTreninzi = _treningRepository.GetAll().ToList();

        var query = sviTreninzi.AsEnumerable();

        if (userId.HasValue)
        {
            query = query.Where(t => t.KorisnikId == userId.Value);
        }

        if (vrsta.HasValue)
        {
            query = query.Where(t => t.Vrsta == vrsta.Value);
        }

        if (period is 7 or 30 or 90)
        {
            var fromDate = DateTime.Now.Date.AddDays(-period);
            query = query.Where(t => t.DatumVrijeme.Date >= fromDate);
        }

        query = sort switch
        {
            "date_asc" => query.OrderBy(t => t.DatumVrijeme),
            "duration_desc" => query.OrderByDescending(t => t.TrajanjeMinuta),
            "duration_asc" => query.OrderBy(t => t.TrajanjeMinuta),
            "rating_desc" => query.OrderByDescending(t => t.Ocjena ?? 0).ThenByDescending(t => t.DatumVrijeme),
            "rating_asc" => query.OrderBy(t => t.Ocjena ?? int.MaxValue).ThenByDescending(t => t.DatumVrijeme),
            _ => query.OrderByDescending(t => t.DatumVrijeme)
        };

        var treninzi = query.ToList();

        var model = new TreningIndexViewModel
        {
            Treninzi = treninzi,
            TotalCount = sviTreninzi.Count,
            TotalMinutes = treninzi.Sum(t => t.TrajanjeMinuta),
            TotalCalories = treninzi.Sum(t => t.PotroseneKalorije ?? 0),
            AverageDuration = treninzi.Count == 0 ? 0 : Math.Round(treninzi.Average(t => t.TrajanjeMinuta), 1),
            AverageRating = treninzi.Any(t => t.Ocjena.HasValue)
                ? Math.Round(treninzi.Where(t => t.Ocjena.HasValue).Average(t => t.Ocjena!.Value), 1)
                : null,
            SelectedUserId = userId,
            SelectedVrsta = vrsta,
            SelectedPeriod = period,
            SelectedSort = sort,
            AvailableUsers = _korisnikRepository.GetAll()
                .Select(k => new TreningUserOption
                {
                    Id = k.Id,
                    FullName = $"{k.Ime} {k.Prezime}",
                    TrainingCount = k.Treninzi.Count
                })
                .OrderBy(o => o.FullName)
                .ToList()
        };

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("filter")]
    public IActionResult Filter(int? userId, VrstaTreninga? vrsta, int period = 0, string sort = "date_desc")
    {
        var sviTreninzi = _treningRepository.GetAll().ToList();
        var query = sviTreninzi.AsEnumerable();

        if (userId.HasValue)
            query = query.Where(t => t.KorisnikId == userId.Value);
        if (vrsta.HasValue)
            query = query.Where(t => t.Vrsta == vrsta.Value);
        if (period is 7 or 30 or 90)
        {
            var fromDate = DateTime.Now.Date.AddDays(-period);
            query = query.Where(t => t.DatumVrijeme.Date >= fromDate);
        }

        query = sort switch
        {
            "date_asc" => query.OrderBy(t => t.DatumVrijeme),
            "duration_desc" => query.OrderByDescending(t => t.TrajanjeMinuta),
            "duration_asc" => query.OrderBy(t => t.TrajanjeMinuta),
            "rating_desc" => query.OrderByDescending(t => t.Ocjena ?? 0).ThenByDescending(t => t.DatumVrijeme),
            "rating_asc" => query.OrderBy(t => t.Ocjena ?? int.MaxValue).ThenByDescending(t => t.DatumVrijeme),
            _ => query.OrderByDescending(t => t.DatumVrijeme)
        };

        var treninzi = query.ToList();
        return Json(new { count = treninzi.Count, treninzi = treninzi.Select(t => new { id = t.Id, naziv = $"{t.DatumVrijeme:dd.MM.yyyy.} — {(Enum.GetName(typeof(VrstaTreninga), t.Vrsta) ?? "Nepoznato")} — {t.TrajanjeMinuta} min" }) });
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "TreningDetails")]
    public IActionResult Details(int id)
    {
        var trening = _treningRepository.GetById(id);

        if (trening is null)
        {
            return NotFound();
        }

        return View(trening);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("novi", Name = "TreningCreate")]
    public IActionResult Create()
    {
        var model = new TreningCreateViewModel
        {
            DatumVrijeme = DateTime.Now
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TreningCreateViewModel model)
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

        // Provjera je li datum u budućnosti ili je to prvi trening
        if (model.DatumVrijeme > DateTime.Now.AddDays(1))
        {
            ModelState.AddModelError(nameof(model.DatumVrijeme), "Datum treninga ne smije biti previše u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var trening = new Trening
        {
            KorisnikId = model.KorisnikId!.Value,
            DatumVrijeme = model.DatumVrijeme,
            TrajanjeMinuta = model.TrajanjeMinuta,
            Vrsta = model.Vrsta,
            Biljeske = model.Biljeske?.Trim(),
            PotroseneKalorije = model.PotroseneKalorije,
            Ocjena = model.Ocjena,
            Korisnik = korisnik!
        };

        _treningRepository.Add(trening);

        return RedirectToRoute("TreningDetails", new { id = trening.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "TreningDelete")]
    public IActionResult Delete(int id)
    {
        var trening = _treningRepository.GetById(id);
        if (trening is null)
            return NotFound();

        return View(trening);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "TreningEdit")]
    public IActionResult Edit(int id)
    {
        var trening = _treningRepository.GetById(id);
        if (trening is null)
            return NotFound();

        // Učitaj korisnika ako nije učitan
        if (trening.Korisnik is null)
        {
            trening.Korisnik = _korisnikRepository.GetById(trening.KorisnikId);
            if (trening.Korisnik is null)
                return NotFound();
        }

        var model = new TreningCreateViewModel
        {
            Id = trening.Id,
            KorisnikId = trening.KorisnikId,
            KorisnikNaziv = $"{trening.Korisnik.Ime} {trening.Korisnik.Prezime}",
            DatumVrijeme = trening.DatumVrijeme,
            TrajanjeMinuta = trening.TrajanjeMinuta,
            Vrsta = trening.Vrsta,
            Biljeske = trening.Biljeske,
            PotroseneKalorije = trening.PotroseneKalorije,
            Ocjena = trening.Ocjena
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "TreningEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TreningCreateViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var trening = _treningRepository.GetById(id);
        if (trening is null)
            return NotFound();

        // Provjera je li korisnik postoji
        if (!model.KorisnikId.HasValue)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Korisnik je obavezan.");
        }
        else if (_korisnikRepository.GetById(model.KorisnikId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Odabrani korisnik ne postoji.");
        }

        // Provjera je li datum u budućnosti ili je to prvi trening
        if (model.DatumVrijeme > DateTime.Now.AddDays(1))
        {
            ModelState.AddModelError(nameof(model.DatumVrijeme), "Datum treninga ne smije biti previše u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        trening.KorisnikId = model.KorisnikId!.Value;
        trening.DatumVrijeme = model.DatumVrijeme;
        trening.TrajanjeMinuta = model.TrajanjeMinuta;
        trening.Vrsta = model.Vrsta;
        trening.Biljeske = model.Biljeske?.Trim();
        trening.PotroseneKalorije = model.PotroseneKalorije;
        trening.Ocjena = model.Ocjena;

        _treningRepository.Update();

        return RedirectToRoute("TreningDetails", new { id = trening.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "TreningDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _treningRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var treninzi = _treningRepository.GetAll()
            .Where(t => (t.Korisnik != null &&
                            (t.Korisnik.Ime.StartsWith(q, StringComparison.OrdinalIgnoreCase) ||
                             t.Korisnik.Prezime.StartsWith(q, StringComparison.OrdinalIgnoreCase))) ||
                        t.DatumVrijeme.ToString("dd.MM.yyyy.").Contains(q))
            .Take(10)
            .Select(t => new
            {
                id = t.Id,
                naziv = $"{t.DatumVrijeme:dd.MM.yyyy. HH:mm} — {t.Vrsta}" +
                        (t.Korisnik != null ? $" — {t.Korisnik.Ime} {t.Korisnik.Prezime}" : "")
            });

        return Json(treninzi);
    }
}
