using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Utilities;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/pretraga")]
public class GlobalSearchController : Controller
{
    private readonly VjezbaRepository _vjezbaRepo;
    private readonly PlanRepository _planRepo;
    private readonly KorisnikRepository _korisnikRepo;

    public GlobalSearchController(
        VjezbaRepository vjezbaRepo,
        PlanRepository planRepo,
        KorisnikRepository korisnikRepo)
    {
        _vjezbaRepo = vjezbaRepo;
        _planRepo = planRepo;
        _korisnikRepo = korisnikRepo;
    }

    [HttpGet]
    public IActionResult Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 1)
            return Json(new
            {
                vjezbe    = Array.Empty<object>(),
                planovi   = Array.Empty<object>(),
                korisnici = Array.Empty<object>()
            });

        var vjezbe = _vjezbaRepo.GetAll()
            .Where(v =>
                v.Naziv.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (v.Opis != null && v.Opis.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Take(5)
            .AsEnumerable()
            .Select(v => new
            {
                id    = v.Id,
                naziv = v.Naziv,
                opis  = v.Opis,
                url   = $"/vjezbe/{v.Id}/{SlugHelper.Slugify(v.Naziv)}"
            })
            .ToList();

        var planovi = _planRepo.GetAll()
            .Where(p =>
                p.Naziv.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (p.Opis != null && p.Opis.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Take(5)
            .AsEnumerable()
            .Select(p => new
            {
                id    = p.Id,
                naziv = p.Naziv,
                opis  = p.Opis,
                url   = $"/planovi/{p.Id}/{SlugHelper.Slugify(p.Naziv)}"
            })
            .ToList();

        var korisnici = _korisnikRepo.GetAll()
            .Where(k =>
                k.Ime.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                k.Prezime.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                k.Email.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .AsEnumerable()
            .Select(k => new
            {
                id    = k.Id,
                naziv = $"{k.Ime} {k.Prezime}",
                opis  = k.Email,
                url   = $"/korisnici/{k.Id}/{SlugHelper.Slugify(k.Ime + "-" + k.Prezime)}"
            })
            .ToList();

        return Json(new { vjezbe, planovi, korisnici });
    }
}
