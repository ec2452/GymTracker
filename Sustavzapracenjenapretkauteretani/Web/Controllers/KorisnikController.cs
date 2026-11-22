using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.Utilities;
using Teretana.Models;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("korisnici")]
public class KorisnikController : BaseController
{
    private readonly KorisnikRepository _korisnikRepository;
    private readonly PlanRepository _planRepository;

    public KorisnikController(
        KorisnikRepository korisnikRepository,
        PlanRepository planRepository,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _korisnikRepository = korisnikRepository;
        _planRepository = planRepository;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "KorisniciIndex")]
    public IActionResult Index()
    {
        return View(_korisnikRepository.GetAll());
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "KorisnikDetails")]
    public IActionResult Details(int id)
    {
        var korisnik = _korisnikRepository.GetById(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        return View(korisnik);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("novi", Name = "KorisnikCreate")]
    public IActionResult Create()
    {
        var model = CreateFormModel();

        PopulatePlanOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(KorisnikCreateViewModel model)
    {
        var korisnici = _korisnikRepository.GetAll();

        if (korisnici.Any(k => k.Email.Equals(model.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Email), "Korisnik s tom email adresom već postoji.");
        }

        if (model.RokCilja <= DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.RokCilja), "Rok cilja mora biti u budućnosti.");
        }

        if (model.DatumMjerenja < model.DatumRodjenja)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne može biti prije datuma rođenja.");
        }

        Plan? selectedPlan = null;
        if (model.PlanId.HasValue)
        {
            selectedPlan = _planRepository.GetById(model.PlanId.Value);
            if (selectedPlan is null)
            {
                ModelState.AddModelError(nameof(model.PlanId), "Odabrani plan ne postoji.");
            }
        }

        if (!ModelState.IsValid)
        {
            PopulatePlanOptions(model);
            return View(model);
        }

        var korisnik = new Korisnik
        {
            Ime = model.Ime.Trim(),
            Prezime = model.Prezime.Trim(),
            Email = model.Email.Trim(),
            DatumRodjenja = model.DatumRodjenja,
            DatumRegistracije = DateTime.Now,
            Visina = model.Visina,
            Tezina = model.Tezina,
            PlanId = model.PlanId,
            TrenutniPlan = selectedPlan
        };

        var cilj = new Cilj
        {
            Tip = model.TipCilja,
            CiljanaVrijednost = model.CiljanaVrijednost,
            Rok = model.RokCilja,
            Postignut = false,
            Korisnik = korisnik
        };

        var mjerenje = new Mjerenje
        {
            DatumMjerenja = model.DatumMjerenja,
            Tezina = model.MjerenjeTezina,
            PostotakMasti = model.PostotakMasti,
            OpsegStruka = model.OpsegStruka,
            Korisnik = korisnik
        };

        korisnik.Ciljevi.Add(cilj);
        korisnik.Mjerenja.Add(mjerenje);
        selectedPlan?.Korisnici.Add(korisnik);

        _korisnikRepository.Add(korisnik);

        return RedirectToRoute("KorisnikDetails", new { id = korisnik.Id, slug = SlugHelper.Slugify($"{korisnik.Ime}-{korisnik.Prezime}") });
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "KorisnikEdit")]
    public IActionResult Edit(int id)
    {
        var korisnik = _korisnikRepository.GetById(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        var model = new KorisnikCreateViewModel
        {
            Id = korisnik.Id,
            Ime = korisnik.Ime,
            Prezime = korisnik.Prezime,
            Email = korisnik.Email,
            DatumRodjenja = korisnik.DatumRodjenja,
            Visina = korisnik.Visina,
            Tezina = korisnik.Tezina,
            PlanId = korisnik.PlanId,
            DatumMjerenja = korisnik.Mjerenja.OrderByDescending(m => m.DatumMjerenja).FirstOrDefault()?.DatumMjerenja ?? DateTime.Today,
            MjerenjeTezina = korisnik.Mjerenja.OrderByDescending(m => m.DatumMjerenja).FirstOrDefault()?.Tezina ?? korisnik.Tezina,
            PostotakMasti = korisnik.Mjerenja.OrderByDescending(m => m.DatumMjerenja).FirstOrDefault()?.PostotakMasti,
            OpsegStruka = korisnik.Mjerenja.OrderByDescending(m => m.DatumMjerenja).FirstOrDefault()?.OpsegStruka,
            TipCilja = korisnik.Ciljevi.OrderByDescending(c => c.Rok).FirstOrDefault()?.Tip ?? TipCilja.Mrsavljenje,
            CiljanaVrijednost = korisnik.Ciljevi.OrderByDescending(c => c.Rok).FirstOrDefault()?.CiljanaVrijednost ?? 0,
            RokCilja = korisnik.Ciljevi.OrderByDescending(c => c.Rok).FirstOrDefault()?.Rok ?? DateTime.Today.AddMonths(3)
        };

        PopulatePlanOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "KorisnikEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, KorisnikCreateViewModel model)
    {
        var korisnik = _korisnikRepository.GetById(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        var korisnici = _korisnikRepository.GetAll();
        if (korisnici.Any(k => k.Id != id && k.Email.Equals(model.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Email), "Korisnik s tom email adresom već postoji.");
        }

        if (model.RokCilja <= DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.RokCilja), "Rok cilja mora biti u budućnosti.");
        }

        if (model.DatumMjerenja < model.DatumRodjenja)
        {
            ModelState.AddModelError(nameof(model.DatumMjerenja), "Datum mjerenja ne može biti prije datuma rođenja.");
        }

        Plan? selectedPlan = null;
        if (model.PlanId.HasValue)
        {
            selectedPlan = _planRepository.GetById(model.PlanId.Value);
            if (selectedPlan is null)
            {
                ModelState.AddModelError(nameof(model.PlanId), "Odabrani plan ne postoji.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.Id = id;
            PopulatePlanOptions(model);
            return View(model);
        }

        var previousPlanId = korisnik.PlanId;

        korisnik.Ime = model.Ime.Trim();
        korisnik.Prezime = model.Prezime.Trim();
        korisnik.Email = model.Email.Trim();
        korisnik.DatumRodjenja = model.DatumRodjenja;
        korisnik.Visina = model.Visina;
        korisnik.Tezina = model.Tezina;
        korisnik.PlanId = model.PlanId;
        korisnik.TrenutniPlan = selectedPlan;

        var latestGoal = korisnik.Ciljevi.OrderByDescending(c => c.Rok).FirstOrDefault();
        if (latestGoal is not null)
        {
            latestGoal.Tip = model.TipCilja;
            latestGoal.CiljanaVrijednost = model.CiljanaVrijednost;
            latestGoal.Rok = model.RokCilja;
        }
        else
        {
            korisnik.Ciljevi.Add(new Cilj
            {
                Tip = model.TipCilja,
                CiljanaVrijednost = model.CiljanaVrijednost,
                Rok = model.RokCilja,
                Postignut = false,
                Korisnik = korisnik
            });
        }

        var latestMeasurement = korisnik.Mjerenja.OrderByDescending(m => m.DatumMjerenja).FirstOrDefault();
        if (latestMeasurement is not null)
        {
            latestMeasurement.DatumMjerenja = model.DatumMjerenja;
            latestMeasurement.Tezina = model.MjerenjeTezina;
            latestMeasurement.PostotakMasti = model.PostotakMasti;
            latestMeasurement.OpsegStruka = model.OpsegStruka;
        }
        else
        {
            korisnik.Mjerenja.Add(new Mjerenje
            {
                DatumMjerenja = model.DatumMjerenja,
                Tezina = model.MjerenjeTezina,
                PostotakMasti = model.PostotakMasti,
                OpsegStruka = model.OpsegStruka,
                Korisnik = korisnik
            });
        }

        if (previousPlanId.HasValue && previousPlanId != model.PlanId)
        {
            var previousPlan = _planRepository.GetById(previousPlanId.Value);
            previousPlan?.Korisnici.Remove(korisnik);
        }

        selectedPlan?.Korisnici.Add(korisnik);
        
        _korisnikRepository.Update();

        return RedirectToRoute("KorisnikDetails", new { id = korisnik.Id, slug = SlugHelper.Slugify($"{korisnik.Ime}-{korisnik.Prezime}") });
    }

    private void PopulatePlanOptions(KorisnikCreateViewModel model)
    {
        model.AvailablePlanovi = _planRepository.GetAll()
            .OrderBy(p => p.Naziv)
            .Select(p => new PlanOption
            {
                Id = p.Id,
                Naziv = p.Naziv
            })
            .ToList();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "KorisnikDelete")]
    public IActionResult Delete(int id)
    {
        var korisnik = _korisnikRepository.GetById(id);
        if (korisnik is null)
            return NotFound();

        return View(korisnik);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "KorisnikDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _korisnikRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var korisnici = _korisnikRepository.GetAll()
            .Where(k => k.Ime.StartsWith(q, StringComparison.OrdinalIgnoreCase) || 
                        k.Prezime.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(k => new { id = k.Id, naziv = $"{k.Ime} {k.Prezime}" });

        return Json(korisnici);
    }

    private static KorisnikCreateViewModel CreateFormModel()
    {
        return new KorisnikCreateViewModel
        {
            DatumRodjenja = DateTime.Today.AddYears(-25),
            DatumMjerenja = DateTime.Today,
            RokCilja = DateTime.Today.AddMonths(3),
            TipCilja = TipCilja.Mrsavljenje
        };
    }
}
