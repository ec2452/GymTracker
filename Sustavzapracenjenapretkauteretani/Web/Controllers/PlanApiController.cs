using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/planovi")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PlanApiController : ControllerBase
{
    private readonly PlanRepository _repo;

    public PlanApiController(PlanRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<PlanDTO>> GetAll(
        [FromQuery] string? naziv = null,
        [FromQuery] RazinaIskustva? razina = null,
        [FromQuery] int? maxTjedana = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(naziv))
            query = query.Where(p => p.Naziv.Contains(naziv, StringComparison.OrdinalIgnoreCase));

        if (razina.HasValue)
            query = query.Where(p => p.Razina == razina.Value);

        if (maxTjedana.HasValue)
            query = query.Where(p => p.TrajanjeTjedana <= maxTjedana.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<PlanDTO>> Search(string q)
    {
        var planovi = _repo.GetAll()
            .Where(p => p.Naziv.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (p.Opis != null && p.Opis.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Select(ToDTO)
            .ToList();
        return Ok(planovi);
    }

    [HttpGet("{id:int}")]
    public ActionResult<PlanDTO> GetById(int id)
    {
        var plan = _repo.GetById(id);
        if (plan is null)
            return NotFound();
        return ToDTO(plan);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer")]
    public ActionResult<PlanDTO> Create(PlanCreateDTO dto)
    {
        var plan = new Plan
        {
            Naziv = dto.Naziv,
            Opis = dto.Opis,
            TrajanjeTjedana = dto.TrajanjeTjedana,
            Razina = dto.Razina,
            DatumKreiranja = DateTime.Now
        };
        _repo.Add(plan);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, ToDTO(plan));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Update(int id, PlanCreateDTO dto)
    {
        var plan = _repo.GetById(id);
        if (plan is null)
            return NotFound();

        plan.Naziv = dto.Naziv;
        plan.Opis = dto.Opis;
        plan.TrajanjeTjedana = dto.TrajanjeTjedana;
        plan.Razina = dto.Razina;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Delete(int id)
    {
        var plan = _repo.GetById(id);
        if (plan is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static PlanDTO ToDTO(Plan p) => new()
    {
        Id = p.Id,
        Naziv = p.Naziv,
        Opis = p.Opis,
        TrajanjeTjedana = p.TrajanjeTjedana,
        Razina = p.Razina.ToString(),
        DatumKreiranja = p.DatumKreiranja,
        Vjezbe = p.Vjezbe.Select(v => new VjezbaDTO
        {
            Id = v.Id,
            Naziv = v.Naziv,
            Opis = v.Opis,
            MisicnaSkupina = v.MisicnaSkupina.ToString(),
            VideoUrl = v.VideoUrl,
            RazinaTezine = v.RazinaTezine,
            PotrebnaOprema = v.PotrebnaOprema
        }).ToList(),
        BrojKorisnika = p.Korisnici.Count
    };
}
