using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/treninzi")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TreningApiController : ControllerBase
{
    private readonly TreningRepository _repo;

    public TreningApiController(TreningRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<TreningDTO>> GetAll(
        [FromQuery] int? korisnikId = null,
        [FromQuery] VrstaTreninga? vrsta = null,
        [FromQuery] DateTime? od = null,
        [FromQuery] DateTime? do_ = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (korisnikId.HasValue)
            query = query.Where(t => t.KorisnikId == korisnikId.Value);

        if (vrsta.HasValue)
            query = query.Where(t => t.Vrsta == vrsta.Value);

        if (od.HasValue)
            query = query.Where(t => t.DatumVrijeme >= od.Value);

        if (do_.HasValue)
            query = query.Where(t => t.DatumVrijeme <= do_.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<TreningDTO>> Search(string q)
    {
        var treninzi = _repo.GetAll()
            .Where(t => t.Biljeske != null && t.Biljeske.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        t.Vrsta.ToString().Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(ToDTO)
            .ToList();
        return Ok(treninzi);
    }

    [HttpGet("{id:int}")]
    public ActionResult<TreningDTO> GetById(int id)
    {
        var trening = _repo.GetById(id);
        if (trening is null)
            return NotFound();
        return ToDTO(trening);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer,User")]
    public ActionResult<TreningDTO> Create(TreningCreateDTO dto)
    {
        var trening = new Trening
        {
            DatumVrijeme = dto.DatumVrijeme,
            TrajanjeMinuta = dto.TrajanjeMinuta,
            Vrsta = dto.Vrsta,
            Biljeske = dto.Biljeske,
            PotroseneKalorije = dto.PotroseneKalorije,
            Ocjena = dto.Ocjena,
            KorisnikId = dto.KorisnikId
        };
        _repo.Add(trening);
        return CreatedAtAction(nameof(GetById), new { id = trening.Id }, ToDTO(trening));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Update(int id, TreningCreateDTO dto)
    {
        var trening = _repo.GetById(id);
        if (trening is null)
            return NotFound();

        trening.DatumVrijeme = dto.DatumVrijeme;
        trening.TrajanjeMinuta = dto.TrajanjeMinuta;
        trening.Vrsta = dto.Vrsta;
        trening.Biljeske = dto.Biljeske;
        trening.PotroseneKalorije = dto.PotroseneKalorije;
        trening.Ocjena = dto.Ocjena;
        trening.KorisnikId = dto.KorisnikId;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Delete(int id)
    {
        var trening = _repo.GetById(id);
        if (trening is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static TreningDTO ToDTO(Trening t) => new()
    {
        Id = t.Id,
        DatumVrijeme = t.DatumVrijeme,
        TrajanjeMinuta = t.TrajanjeMinuta,
        Vrsta = t.Vrsta.ToString(),
        Biljeske = t.Biljeske,
        PotroseneKalorije = t.PotroseneKalorije,
        Ocjena = t.Ocjena,
        KorisnikId = t.KorisnikId
    };
}
