using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/mjerenja")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MjerenjeApiController : ControllerBase
{
    private readonly MjerenjeRepository _repo;

    public MjerenjeApiController(MjerenjeRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<MjerenjeDTO>> GetAll(
        [FromQuery] int? korisnikId = null,
        [FromQuery] DateTime? od = null,
        [FromQuery] DateTime? do_ = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (korisnikId.HasValue)
            query = query.Where(m => m.KorisnikId == korisnikId.Value);

        if (od.HasValue)
            query = query.Where(m => m.DatumMjerenja >= od.Value);

        if (do_.HasValue)
            query = query.Where(m => m.DatumMjerenja <= do_.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<MjerenjeDTO>> Search(string q)
    {
        if (!double.TryParse(q, out var broj))
            return Ok(new List<MjerenjeDTO>());

        var mjerenja = _repo.GetAll()
            .Where(m => m.Tezina == broj ||
                        (m.PostotakMasti.HasValue && m.PostotakMasti == broj) ||
                        (m.OpsegStruka.HasValue && m.OpsegStruka == broj))
            .Select(ToDTO)
            .ToList();
        return Ok(mjerenja);
    }

    [HttpGet("{id:int}")]
    public ActionResult<MjerenjeDTO> GetById(int id)
    {
        var mjerenje = _repo.GetById(id);
        if (mjerenje is null)
            return NotFound();
        return ToDTO(mjerenje);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer,User")]
    public ActionResult<MjerenjeDTO> Create(MjerenjeCreateDTO dto)
    {
        var mjerenje = new Mjerenje
        {
            DatumMjerenja = dto.DatumMjerenja,
            Tezina = dto.Tezina,
            PostotakMasti = dto.PostotakMasti,
            OpsegStruka = dto.OpsegStruka,
            KorisnikId = dto.KorisnikId
        };
        _repo.Add(mjerenje);
        return CreatedAtAction(nameof(GetById), new { id = mjerenje.Id }, ToDTO(mjerenje));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Update(int id, MjerenjeCreateDTO dto)
    {
        var mjerenje = _repo.GetById(id);
        if (mjerenje is null)
            return NotFound();

        mjerenje.DatumMjerenja = dto.DatumMjerenja;
        mjerenje.Tezina = dto.Tezina;
        mjerenje.PostotakMasti = dto.PostotakMasti;
        mjerenje.OpsegStruka = dto.OpsegStruka;
        mjerenje.KorisnikId = dto.KorisnikId;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer,User")]
    public IActionResult Delete(int id)
    {
        var mjerenje = _repo.GetById(id);
        if (mjerenje is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static MjerenjeDTO ToDTO(Mjerenje m) => new()
    {
        Id = m.Id,
        DatumMjerenja = m.DatumMjerenja,
        Tezina = m.Tezina,
        PostotakMasti = m.PostotakMasti,
        OpsegStruka = m.OpsegStruka,
        KorisnikId = m.KorisnikId
    };
}
