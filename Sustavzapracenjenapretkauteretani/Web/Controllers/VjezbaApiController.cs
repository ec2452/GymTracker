using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/vjezbe")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class VjezbaApiController : ControllerBase
{
    private readonly VjezbaRepository _repo;

    public VjezbaApiController(VjezbaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<VjezbaDTO>> GetAll(
        [FromQuery] string? naziv = null,
        [FromQuery] MisicnaSkupina? misicnaSkupina = null,
        [FromQuery] int? razinaTezine = null,
        [FromQuery] bool? potrebnaOprema = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(naziv))
            query = query.Where(v => v.Naziv.Contains(naziv, StringComparison.OrdinalIgnoreCase));

        if (misicnaSkupina.HasValue)
            query = query.Where(v => v.MisicnaSkupina == misicnaSkupina.Value);

        if (razinaTezine.HasValue)
            query = query.Where(v => v.RazinaTezine == razinaTezine.Value);

        if (potrebnaOprema.HasValue)
            query = query.Where(v => v.PotrebnaOprema == potrebnaOprema.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<VjezbaDTO>> Search(string q)
    {
        var vjezbe = _repo.GetAll()
            .Where(v => v.Naziv.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (v.Opis != null && v.Opis.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Select(ToDTO)
            .ToList();
        return Ok(vjezbe);
    }

    [HttpGet("{id:int}")]
    public ActionResult<VjezbaDTO> GetById(int id)
    {
        var vjezba = _repo.GetById(id);
        if (vjezba is null)
            return NotFound();
        return ToDTO(vjezba);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer")]
    public ActionResult<VjezbaDTO> Create(VjezbaCreateDTO dto)
    {
        var vjezba = new Vjezba
        {
            Naziv = dto.Naziv,
            Opis = dto.Opis,
            MisicnaSkupina = dto.MisicnaSkupina,
            VideoUrl = dto.VideoUrl,
            RazinaTezine = dto.RazinaTezine,
            PotrebnaOprema = dto.PotrebnaOprema
        };
        _repo.Add(vjezba);
        return CreatedAtAction(nameof(GetById), new { id = vjezba.Id }, ToDTO(vjezba));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Update(int id, VjezbaCreateDTO dto)
    {
        var vjezba = _repo.GetById(id);
        if (vjezba is null)
            return NotFound();

        vjezba.Naziv = dto.Naziv;
        vjezba.Opis = dto.Opis;
        vjezba.MisicnaSkupina = dto.MisicnaSkupina;
        vjezba.VideoUrl = dto.VideoUrl;
        vjezba.RazinaTezine = dto.RazinaTezine;
        vjezba.PotrebnaOprema = dto.PotrebnaOprema;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Delete(int id)
    {
        var vjezba = _repo.GetById(id);
        if (vjezba is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static VjezbaDTO ToDTO(Vjezba v) => new()
    {
        Id = v.Id,
        Naziv = v.Naziv,
        Opis = v.Opis,
        MisicnaSkupina = v.MisicnaSkupina.ToString(),
        VideoUrl = v.VideoUrl,
        RazinaTezine = v.RazinaTezine,
        PotrebnaOprema = v.PotrebnaOprema
    };
}
