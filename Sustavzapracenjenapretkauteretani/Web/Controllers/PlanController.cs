using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("planovi")]
public class PlanController : BaseController
{
    private readonly PlanRepository _planRepository;
    private readonly KorisnikRepository _korisnikRepository;

    public PlanController(PlanRepository planRepository, KorisnikRepository korisnikRepository,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _planRepository = planRepository;
        _korisnikRepository = korisnikRepository;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "PlanoviIndex")]
    public IActionResult Index()
    {
        return View(_planRepository.GetAll().ToList());
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "PlanDetails")]
    public IActionResult Details(int id)
    {
        var plan = _planRepository.GetById(id);
        if (plan is null)
        {
            return NotFound();
        }

        var aktivniKorisnici = _korisnikRepository.GetAll()
            .Where(k => k.PlanId == plan.Id || (k.TrenutniPlan != null && k.TrenutniPlan.Id == plan.Id))
            .OrderBy(k => k.Ime)
            .ThenBy(k => k.Prezime)
            .ToList();

        plan.Korisnici.Clear();
        foreach (var korisnik in aktivniKorisnici)
        {
            plan.Korisnici.Add(korisnik);
        }

        return View(plan);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("novi", Name = "PlanCreate")]
    public IActionResult Create()
    {
        return View(new PlanCreateViewModel());
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PlanCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var trimmedName = model.Naziv.Trim();
        if (_planRepository.GetAll().Any(p => p.Naziv.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Naziv), "Plan s tim nazivom već postoji.");
            return View(model);
        }

        var plan = new Plan
        {
            Naziv = trimmedName,
            Opis = model.Opis?.Trim(),
            TrajanjeTjedana = model.TrajanjeTjedana,
            Razina = model.Razina,
            DatumKreiranja = DateTime.Now
        };

        _planRepository.Add(plan);

        return RedirectToRoute("PlanDetails", new { id = plan.Id });
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "PlanEdit")]
    public IActionResult Edit(int id)
    {
        var plan = _planRepository.GetById(id);
        if (plan is null)
            return NotFound();

        var model = new PlanCreateViewModel
        {
            Id = plan.Id,
            Naziv = plan.Naziv,
            Opis = plan.Opis,
            TrajanjeTjedana = plan.TrajanjeTjedana,
            Razina = plan.Razina
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "PlanEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PlanCreateViewModel model)
    {
        var plan = _planRepository.GetById(id);
        if (plan is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var trimmedName = model.Naziv.Trim();
        if (_planRepository.GetAll().Any(p => p.Id != id && p.Naziv.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Naziv), "Plan s tim nazivom već postoji.");
            model.Id = id;
            return View(model);
        }

        plan.Naziv = trimmedName;
        plan.Opis = model.Opis?.Trim();
        plan.TrajanjeTjedana = model.TrajanjeTjedana;
        plan.Razina = model.Razina;

        _planRepository.Update();

        return RedirectToRoute("PlanDetails", new { id = plan.Id });
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var planovi = _planRepository.GetAll()
            .Where(p => p.Naziv.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(p => new { id = p.Id, naziv = p.Naziv });

        return Json(planovi);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "PlanDelete")]
    public IActionResult Delete(int id)
    {
        var plan = _planRepository.GetById(id);
        if (plan is null)
            return NotFound();

        return View(plan);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "PlanDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _planRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
