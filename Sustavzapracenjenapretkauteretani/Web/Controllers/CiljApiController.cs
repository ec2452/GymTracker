using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/ciljevi")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CiljApiController : ControllerBase
{
    private readonly CiljRepository _repo;

    public CiljApiController(CiljRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<CiljDTO>> GetAll(
        [FromQuery] int? korisnikId = null,
        [FromQuery] TipCilja? tip = null,
        [FromQuery] bool? postignut = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (korisnikId.HasValue)
            query = query.Where(c => c.KorisnikId == korisnikId.Value);

        if (tip.HasValue)
            query = query.Where(c => c.Tip == tip.Value);

        if (postignut.HasValue)
            query = query.Where(c => c.Postignut == postignut.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<CiljDTO>> Search(string q)
    {
        var ciljevi = _repo.GetAll()
            .Where(c => c.Tip.ToString().Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(ToDTO)
            .ToList();
        return Ok(ciljevi);
    }

    [HttpGet("{id:int}")]
    public ActionResult<CiljDTO> GetById(int id)
    {
        var cilj = _repo.GetById(id);
        if (cilj is null)
            return NotFound();
        return ToDTO(cilj);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer,User")]
    public ActionResult<CiljDTO> Create(CiljCreateDTO dto)
    {
        var cilj = new Cilj
        {
            Tip = dto.Tip,
            CiljanaVrijednost = dto.CiljanaVrijednost,
            Rok = dto.Rok,
            Postignut = dto.Postignut,
            KorisnikId = dto.KorisnikId
        };
        _repo.Add(cilj);
        return CreatedAtAction(nameof(GetById), new { id = cilj.Id }, ToDTO(cilj));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Update(int id, CiljCreateDTO dto)
    {
        var cilj = _repo.GetById(id);
        if (cilj is null)
            return NotFound();

        cilj.Tip = dto.Tip;
        cilj.CiljanaVrijednost = dto.CiljanaVrijednost;
        cilj.Rok = dto.Rok;
        cilj.Postignut = dto.Postignut;
        cilj.KorisnikId = dto.KorisnikId;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Delete(int id)
    {
        var cilj = _repo.GetById(id);
        if (cilj is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static CiljDTO ToDTO(Cilj c) => new()
    {
        Id = c.Id,
        Tip = c.Tip.ToString(),
        CiljanaVrijednost = c.CiljanaVrijednost,
        Rok = c.Rok,
        Postignut = c.Postignut,
        KorisnikId = c.KorisnikId
    };
}
