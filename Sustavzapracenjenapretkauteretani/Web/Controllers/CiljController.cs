using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("ciljevi")]
public class CiljController : BaseController
{
    private readonly CiljRepository _ciljRepository;
    private readonly KorisnikRepository _korisnikRepository;

    public CiljController(CiljRepository ciljRepository, KorisnikRepository korisnikRepository,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _ciljRepository = ciljRepository;
        _korisnikRepository = korisnikRepository;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "CiljeviIndex")]
    public IActionResult Index()
    {
        var ciljevi = _ciljRepository.GetAll().ToList();

        return View(ciljevi);
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "CiljDetails")]
    public IActionResult Details(int id)
    {
        var cilj = _ciljRepository.GetById(id);

        if (cilj is null) return NotFound();

        return View(cilj);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("novi", Name = "CiljCreate")]
    public IActionResult Create()
    {
        var model = new CiljCreateViewModel
        {
            Rok = DateTime.Today.AddMonths(3)
        };

        PopulateKorisnikOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("novi")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CiljCreateViewModel model)
    {
        // Provjera je li korisnik postoji
        if (!model.KorisnikId.HasValue)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Korisnik je obavezan.");
        }
        else if (_korisnikRepository.GetById(model.KorisnikId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.KorisnikId), "Odabrani korisnik ne postoji.");
        }

        // Provjera je li rok u budućnosti
        if (model.Rok <= DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Rok), "Rok mora biti u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            PopulateKorisnikOptions(model);
            return View(model);
        }

        var cilj = new Cilj
        {
            KorisnikId = model.KorisnikId!.Value,
            Tip = model.Tip,
            CiljanaVrijednost = model.CiljanaVrijednost,
            Rok = model.Rok,
            Postignut = model.Postignut
        };

        _ciljRepository.Add(cilj);

        return RedirectToRoute("CiljDetails", new { id = cilj.Id });
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "CiljEdit")]
    public IActionResult Edit(int id)
    {
        var cilj = _ciljRepository.GetById(id);
        if (cilj is null)
            return NotFound();

        var model = new CiljCreateViewModel
        {
            Id = cilj.Id,
            KorisnikId = cilj.KorisnikId,
            Tip = cilj.Tip,
            CiljanaVrijednost = cilj.CiljanaVrijednost,
            Rok = cilj.Rok,
            Postignut = cilj.Postignut,
            KorisnikNaziv = cilj.Korisnik != null ? $"{cilj.Korisnik.Ime} {cilj.Korisnik.Prezime}" : string.Empty
        };

        PopulateKorisnikOptions(model);
        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "CiljEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CiljCreateViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var cilj = _ciljRepository.GetById(id);
        if (cilj is null)
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

        // Provjera je li rok u budućnosti
        if (model.Rok <= DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Rok), "Rok mora biti u budućnosti.");
        }

        if (!ModelState.IsValid)
        {
            PopulateKorisnikOptions(model);
            return View(model);
        }

        cilj.KorisnikId = model.KorisnikId!.Value;
        cilj.Tip = model.Tip;
        cilj.CiljanaVrijednost = model.CiljanaVrijednost;
        cilj.Rok = model.Rok;
        cilj.Postignut = model.Postignut;

        _ciljRepository.Update();

        return RedirectToRoute("CiljDetails", new { id = cilj.Id });
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var ciljevi = _ciljRepository.GetAll()
            .Where(c => c.Tip.ToString().StartsWith(q, StringComparison.OrdinalIgnoreCase) || 
                        (c.Korisnik != null && (c.Korisnik.Ime.StartsWith(q, StringComparison.OrdinalIgnoreCase) || 
                                                c.Korisnik.Prezime.StartsWith(q, StringComparison.OrdinalIgnoreCase))))
            .Take(10)
            .Select(c => new { id = c.Id, naziv = $"{c.Tip} — {(c.Korisnik != null ? $"{c.Korisnik.Ime} {c.Korisnik.Prezime}" : "Nepoznat")}" });

        return Json(ciljevi);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "CiljDelete")]
    public IActionResult Delete(int id)
    {
        var cilj = _ciljRepository.GetById(id);
        if (cilj is null)
            return NotFound();

        return View(cilj);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "CiljDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _ciljRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateKorisnikOptions(CiljCreateViewModel model)
    {
        model.AvailableKorisnici = _korisnikRepository.GetAll()
            .OrderBy(k => k.Prezime)
            .ThenBy(k => k.Ime)
            .Select(k => new KorisnikOption
            {
                Id = k.Id,
                ImeIPrezime = $"{k.Ime} {k.Prezime}"
            })
            .ToList();
    }
}


