using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Authorize]
[Route("trening-vjezbe")]
public class TreningVjezbaController : Controller
{
    private readonly TreningVjezbaRepository _treningVjezbaRepository;
    private readonly TreningRepository _treningRepository;
    private readonly VjezbaRepository _vjezbaRepository;

    public TreningVjezbaController(TreningVjezbaRepository treningVjezbaRepository, TreningRepository treningRepository, VjezbaRepository vjezbaRepository)
    {
        _treningVjezbaRepository = treningVjezbaRepository;
        _treningRepository = treningRepository;
        _vjezbaRepository = vjezbaRepository;
    }

    [HttpGet("", Name = "TreningVjezbeIndex")]
    public IActionResult Index()
    {
        var treningVjezbe = _treningVjezbaRepository.GetAll().ToList();

        return View(treningVjezbe);
    }

    [HttpGet("{id:int}/{slug?}", Name = "TreningVjezbaDetails")]
    public IActionResult Details(int id)
    {
        var treningVjezba = _treningVjezbaRepository.GetById(id);

        if (treningVjezba is null)
        {
            return NotFound();
        }

        return View(treningVjezba);
    }

    [HttpGet("nova", Name = "TreningVjezbaCreate")]
    public IActionResult Create()
    {
        var model = new TreningVjezbaCreateViewModel
        {
            OdmorSekundi = 60,
            Redoslijed = 1
        };

        return View(model);
    }

    [HttpPost("nova")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TreningVjezbaCreateViewModel model)
    {
        // Provjera je li trening postoji
        if (!model.TreningId.HasValue)
        {
            ModelState.AddModelError(nameof(model.TreningId), "Trening je obavezan.");
        }
        else if (_treningRepository.GetById(model.TreningId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.TreningId), "Odabrani trening ne postoji.");
        }

        // Provjera je li vježba postoji
        if (!model.VjezbaId.HasValue)
        {
            ModelState.AddModelError(nameof(model.VjezbaId), "Vježba je obavezna.");
        }
        else if (_vjezbaRepository.GetById(model.VjezbaId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.VjezbaId), "Odabrana vježba ne postoji.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var treningVjezba = new TreningVjezba
        {
            TreningId = model.TreningId!.Value,
            VjezbaId = model.VjezbaId!.Value,
            BrojSerija = model.BrojSerija,
            BrojPonavljanja = model.BrojPonavljanja,
            TezinaKg = model.TezinaKg,
            OdmorSekundi = model.OdmorSekundi,
            Redoslijed = model.Redoslijed
        };

        _treningVjezbaRepository.Add(treningVjezba);

        return RedirectToRoute("TreningVjezbaDetails", new { id = treningVjezba.Id });
    }

    [HttpGet("{id:int}/{slug?}/uredi", Name = "TreningVjezbaEdit")]
    public IActionResult Edit(int id)
    {
        var treningVjezba = _treningVjezbaRepository.GetById(id);
        if (treningVjezba is null)
            return NotFound();

        var model = new TreningVjezbaCreateViewModel
        {
            Id = treningVjezba.Id,
            TreningId = treningVjezba.TreningId,
            VjezbaId = treningVjezba.VjezbaId,
            BrojSerija = treningVjezba.BrojSerija,
            BrojPonavljanja = treningVjezba.BrojPonavljanja,
            TezinaKg = treningVjezba.TezinaKg,
            OdmorSekundi = treningVjezba.OdmorSekundi,
            Redoslijed = treningVjezba.Redoslijed,
            TreningDisplayText = treningVjezba.Trening != null
                ? $"{treningVjezba.Trening.DatumVrijeme:dd.MM.yyyy. HH:mm} — {treningVjezba.Trening.Vrsta}"
                : string.Empty,
            VjezbaDisplayText = treningVjezba.Vjezba != null ? treningVjezba.Vjezba.Naziv : string.Empty
        };

        return View(model);
    }

    [HttpPost("{id:int}/{slug?}/uredi", Name = "TreningVjezbaEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TreningVjezbaCreateViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        var treningVjezba = _treningVjezbaRepository.GetById(id);
        if (treningVjezba is null)
            return NotFound();

        // Provjera je li trening postoji
        if (!model.TreningId.HasValue)
        {
            ModelState.AddModelError(nameof(model.TreningId), "Trening je obavezan.");
        }
        else if (_treningRepository.GetById(model.TreningId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.TreningId), "Odabrani trening ne postoji.");
        }

        // Provjera je li vježba postoji
        if (!model.VjezbaId.HasValue)
        {
            ModelState.AddModelError(nameof(model.VjezbaId), "Vježba je obavezna.");
        }
        else if (_vjezbaRepository.GetById(model.VjezbaId.Value) is null)
        {
            ModelState.AddModelError(nameof(model.VjezbaId), "Odabrana vježba ne postoji.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        treningVjezba.TreningId = model.TreningId!.Value;
        treningVjezba.VjezbaId = model.VjezbaId!.Value;
        treningVjezba.BrojSerija = model.BrojSerija;
        treningVjezba.BrojPonavljanja = model.BrojPonavljanja;
        treningVjezba.TezinaKg = model.TezinaKg;
        treningVjezba.OdmorSekundi = model.OdmorSekundi;
        treningVjezba.Redoslijed = model.Redoslijed;

        _treningVjezbaRepository.Update();

        return RedirectToRoute("TreningVjezbaDetails", new { id = treningVjezba.Id });
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var treningVjezbe = _treningVjezbaRepository.GetAll()
            .Where(tv => (tv.Vjezba != null && tv.Vjezba.Naziv.StartsWith(q, StringComparison.OrdinalIgnoreCase)) || 
                         (tv.Trening != null && tv.Trening.DatumVrijeme.ToString("yyyy-MM-dd").StartsWith(q, StringComparison.OrdinalIgnoreCase)))
            .Take(10)
            .Select(tv => new { id = tv.Id, naziv = $"{(tv.Vjezba != null ? tv.Vjezba.Naziv : "Nepoznata")} — Trening {tv.TreningId}" });

        return Json(treningVjezbe);
    }

    [HttpGet("{id:int}/{slug?}/obrisi", Name = "TreningVjezbaDelete")]
    public IActionResult Delete(int id)
    {
        var treningVjezba = _treningVjezbaRepository.GetById(id);
        if (treningVjezba is null)
            return NotFound();

        return View(treningVjezba);
    }

    [HttpPost("{id:int}/{slug?}/obrisi", Name = "TreningVjezbaDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _treningVjezbaRepository.Delete(id);
        return RedirectToAction(nameof(Index));
    }

}
