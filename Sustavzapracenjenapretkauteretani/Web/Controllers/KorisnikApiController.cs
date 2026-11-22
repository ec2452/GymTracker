using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/korisnici")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class KorisnikApiController : ControllerBase
{
    private readonly KorisnikRepository _repo;

    public KorisnikApiController(KorisnikRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<KorisnikDTO>> GetAll(
        [FromQuery] string? pretraga = null,
        [FromQuery] int? planId = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(pretraga))
            query = query.Where(k =>
                k.Ime.Contains(pretraga, StringComparison.OrdinalIgnoreCase) ||
                k.Prezime.Contains(pretraga, StringComparison.OrdinalIgnoreCase) ||
                k.Email.Contains(pretraga, StringComparison.OrdinalIgnoreCase));

        if (planId.HasValue)
            query = query.Where(k => k.PlanId == planId.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<KorisnikDTO>> Search(string q)
    {
        var korisnici = _repo.GetAll()
            .Where(k => k.Ime.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        k.Prezime.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        k.Email.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(ToDTO)
            .ToList();
        return Ok(korisnici);
    }

    [HttpGet("{id:int}")]
    public ActionResult<KorisnikDTO> GetById(int id)
    {
        var korisnik = _repo.GetById(id);
        if (korisnik is null)
            return NotFound();
        return ToDTO(korisnik);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer")]
    public ActionResult<KorisnikDTO> Create(KorisnikCreateDTO dto)
    {
        var korisnik = new Korisnik
        {
            Ime = dto.Ime,
            Prezime = dto.Prezime,
            Email = dto.Email,
            DatumRodjenja = dto.DatumRodjenja,
            Visina = dto.Visina,
            Tezina = dto.Tezina,
            PlanId = dto.PlanId,
            DatumRegistracije = DateTime.Now
        };
        _repo.Add(korisnik);
        return CreatedAtAction(nameof(GetById), new { id = korisnik.Id }, ToDTO(korisnik));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Update(int id, KorisnikCreateDTO dto)
    {
        var korisnik = _repo.GetById(id);
        if (korisnik is null)
            return NotFound();

        korisnik.Ime = dto.Ime;
        korisnik.Prezime = dto.Prezime;
        korisnik.Email = dto.Email;
        korisnik.DatumRodjenja = dto.DatumRodjenja;
        korisnik.Visina = dto.Visina;
        korisnik.Tezina = dto.Tezina;
        korisnik.PlanId = dto.PlanId;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Delete(int id)
    {
        var korisnik = _repo.GetById(id);
        if (korisnik is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static KorisnikDTO ToDTO(Korisnik k) => new()
    {
        Id = k.Id,
        Ime = k.Ime,
        Prezime = k.Prezime,
        Email = k.Email,
        DatumRodjenja = k.DatumRodjenja,
        DatumRegistracije = k.DatumRegistracije,
        Visina = k.Visina,
        Tezina = k.Tezina,
        PlanId = k.PlanId
    };
}
