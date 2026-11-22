using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/trening-vjezbe")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TreningVjezbaApiController : ControllerBase
{
    private readonly TreningVjezbaRepository _repo;

    public TreningVjezbaApiController(TreningVjezbaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<List<TreningVjezbaDTO>> GetAll(
        [FromQuery] int? treningId = null,
        [FromQuery] int? vjezbaId = null)
    {
        var query = _repo.GetAll().AsEnumerable();

        if (treningId.HasValue)
            query = query.Where(tv => tv.TreningId == treningId.Value);

        if (vjezbaId.HasValue)
            query = query.Where(tv => tv.VjezbaId == vjezbaId.Value);

        return query.Select(ToDTO).ToList();
    }

    [HttpGet("pretraga/{q}")]
    public ActionResult<List<TreningVjezbaDTO>> Search(string q)
    {
        var rezultati = _repo.GetAll()
            .Where(tv => tv.Vjezba != null &&
                         tv.Vjezba.Naziv.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(ToDTO)
            .ToList();
        return Ok(rezultati);
    }

    [HttpGet("{id:int}")]
    public ActionResult<TreningVjezbaDTO> GetById(int id)
    {
        var tv = _repo.GetById(id);
        if (tv is null)
            return NotFound();
        return ToDTO(tv);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Trainer")]
    public ActionResult<TreningVjezbaDTO> Create(TreningVjezbaCreateDTO dto)
    {
        var tv = new TreningVjezba
        {
            TreningId = dto.TreningId,
            VjezbaId = dto.VjezbaId,
            BrojSerija = dto.BrojSerija,
            BrojPonavljanja = dto.BrojPonavljanja,
            TezinaKg = dto.TezinaKg,
            OdmorSekundi = dto.OdmorSekundi,
            Redoslijed = dto.Redoslijed
        };
        _repo.Add(tv);
        return CreatedAtAction(nameof(GetById), new { id = tv.Id }, ToDTO(tv));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Update(int id, TreningVjezbaCreateDTO dto)
    {
        var tv = _repo.GetById(id);
        if (tv is null)
            return NotFound();

        tv.TreningId = dto.TreningId;
        tv.VjezbaId = dto.VjezbaId;
        tv.BrojSerija = dto.BrojSerija;
        tv.BrojPonavljanja = dto.BrojPonavljanja;
        tv.TezinaKg = dto.TezinaKg;
        tv.OdmorSekundi = dto.OdmorSekundi;
        tv.Redoslijed = dto.Redoslijed;
        _repo.Update();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public IActionResult Delete(int id)
    {
        var tv = _repo.GetById(id);
        if (tv is null)
            return NotFound();
        _repo.Delete(id);
        return NoContent();
    }

    private static TreningVjezbaDTO ToDTO(TreningVjezba tv) => new()
    {
        Id = tv.Id,
        TreningId = tv.TreningId,
        Vjezba = tv.Vjezba == null ? null : new VjezbaDTO
        {
            Id = tv.Vjezba.Id,
            Naziv = tv.Vjezba.Naziv,
            Opis = tv.Vjezba.Opis,
            MisicnaSkupina = tv.Vjezba.MisicnaSkupina.ToString(),
            VideoUrl = tv.Vjezba.VideoUrl,
            RazinaTezine = tv.Vjezba.RazinaTezine,
            PotrebnaOprema = tv.Vjezba.PotrebnaOprema
        },
        BrojSerija = tv.BrojSerija,
        BrojPonavljanja = tv.BrojPonavljanja,
        TezinaKg = tv.TezinaKg,
        OdmorSekundi = tv.OdmorSekundi,
        Redoslijed = tv.Redoslijed
    };
}
