using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.Utilities;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.DAL;
using Teretana.Models;
using Microsoft.EntityFrameworkCore;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("vjezbe")]
public class VjezbaController : BaseController
{
    private static readonly string[] DozvoljeneTipovi = { "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf" };
    private static readonly string[] DozvoljeneEkstenzije = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };
    private const long MaxVelicinaBytes = 10 * 1024 * 1024; // 10 MB

    private readonly VjezbaRepository _vjezbaRepository;
    private readonly TeretanaDbContext _dbContext;

    public VjezbaController(VjezbaRepository vjezbaRepository, TeretanaDbContext dbContext,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _vjezbaRepository = vjezbaRepository;
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet("", Name = "VjezbeIndex")]
    public IActionResult Index()
    {
        return View(_vjezbaRepository.GetAll());
    }

    [Authorize]
    [HttpGet("{id:int}/{slug?}", Name = "VjezbaDetails")]
    public IActionResult Details(int id)
    {
        var vjezba = _vjezbaRepository.GetById(id);
        if (vjezba is null)
        {
            return NotFound();
        }

        return View(vjezba);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("nova", Name = "VjezbaCreate")]
    public IActionResult Create()
    {
        return View(new VjezbaCreateViewModel());
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("nova")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(VjezbaCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var trimmedName = model.Naziv.Trim();
        if (_vjezbaRepository.GetAll().Any(v => v.Naziv.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Naziv), "Vježba s tim nazivom već postoji.");
            return View(model);
        }

        var vjezba = new Vjezba
        {
            Naziv = trimmedName,
            Opis = model.Opis?.Trim(),
            MisicnaSkupina = model.MisicnaSkupina,
            VideoUrl = model.VideoUrl?.Trim(),
            RazinaTezine = model.RazinaTezine,
            PotrebnaOprema = model.PotrebnaOprema
        };

        _vjezbaRepository.Add(vjezba);
        return RedirectToRoute("VjezbaDetails", new { id = vjezba.Id, slug = SlugHelper.Slugify(vjezba.Naziv) });
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpGet("{id:int}/{slug?}/uredi", Name = "VjezbaEdit")]
    public IActionResult Edit(int id)
    {
        var vjezba = _vjezbaRepository.GetById(id);
        if (vjezba is null)
            return NotFound();

        var model = new VjezbaCreateViewModel
        {
            Id = vjezba.Id,
            Naziv = vjezba.Naziv,
            Opis = vjezba.Opis,
            MisicnaSkupina = vjezba.MisicnaSkupina,
            VideoUrl = vjezba.VideoUrl,
            RazinaTezine = vjezba.RazinaTezine,
            PotrebnaOprema = vjezba.PotrebnaOprema
        };

        ViewBag.Attachments = _dbContext.Attachments
            .Where(a => a.VjezbaId == id)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        return View(model);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/{slug?}/uredi", Name = "VjezbaEditPost")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, VjezbaCreateViewModel model)
    {
        var vjezba = _vjezbaRepository.GetById(id);
        if (vjezba is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Id = id;
            return View(model);
        }

        var trimmedName = model.Naziv.Trim();
        if (_vjezbaRepository.GetAll().Any(v => v.Id != id && v.Naziv.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Naziv), "Vježba s tim nazivom već postoji.");
            model.Id = id;
            return View(model);
        }

        vjezba.Naziv = trimmedName;
        vjezba.Opis = model.Opis?.Trim();
        vjezba.MisicnaSkupina = model.MisicnaSkupina;
        vjezba.VideoUrl = model.VideoUrl?.Trim();
        vjezba.RazinaTezine = model.RazinaTezine;
        vjezba.PotrebnaOprema = model.PotrebnaOprema;

        _vjezbaRepository.Update();
        return RedirectToRoute("VjezbaDetails", new { id = vjezba.Id, slug = SlugHelper.Slugify(vjezba.Naziv) });
    }

    [AllowAnonymous]
    [HttpGet("pretraga")]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var vjezbe = _vjezbaRepository.GetAll()
            .Where(v => v.Naziv.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(v => new { id = v.Id, naziv = v.Naziv });

        return Json(vjezbe);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("{id:int}/upload", Name = "VjezbaUpload")]
    public IActionResult UploadAttachment(int id, IFormFile file)
    {
        var vjezba = _vjezbaRepository.GetById(id);
        if (vjezba is null)
            return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest("Datoteka nije odabrana.");

        if (file.Length > MaxVelicinaBytes)
            return BadRequest("Datoteka je prevelika. Maksimalna veličina je 10 MB.");

        var ekstenzija = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!DozvoljeneEkstenzije.Contains(ekstenzija))
            return BadRequest("Nedozvoljena vrsta datoteke. Dozvoljeni su: slike i PDF.");

        if (!DozvoljeneTipovi.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Nedozvoljen MIME tip datoteke.");

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "vjezbe", id.ToString());
        Directory.CreateDirectory(uploadDir);

        var novoIme = Guid.NewGuid().ToString() + ekstenzija;
        var putanjaFizicka = Path.Combine(uploadDir, novoIme);

        using (var stream = new FileStream(putanjaFizicka, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        var attachment = new Attachment
        {
            VjezbaId = id,
            FileName = file.FileName,
            FilePath = $"/uploads/vjezbe/{id}/{novoIme}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Attachments.Add(attachment);
        _dbContext.SaveChanges();

        return Json(new { success = true, id = attachment.Id, fileName = attachment.FileName });
    }

    [Authorize]
    [HttpGet("{id:int}/attachments", Name = "VjezbaAttachments")]
    public IActionResult GetAttachments(int id)
    {
        var attachments = _dbContext.Attachments
            .Where(a => a.VjezbaId == id)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        return PartialView("_AttachmentList", attachments);
    }

    [Authorize(Roles = "Admin,Trainer")]
    [HttpPost("obrisi-privitak", Name = "AttachmentDelete")]
    public IActionResult DeleteAttachment(int id)
    {
        var attachment = _dbContext.Attachments.FirstOrDefault(a => a.Id == id);
        if (attachment is null)
            return NotFound();

        var fizicka = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(fizicka))
            System.IO.File.Delete(fizicka);

        _dbContext.Attachments.Remove(attachment);
        _dbContext.SaveChanges();

        return Json(new { success = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}/{slug?}/obrisi", Name = "VjezbaDelete")]
    public IActionResult Delete(int id)
    {
        var vjezba = _vjezbaRepository.GetById(id);
        if (vjezba is null)
            return NotFound();

        return View(vjezba);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/{slug?}/obrisi", Name = "VjezbaDeletePost")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _vjezbaRepository.Delete(id);
        return RedirectToRoute("VjezbeIndex");
    }
}
