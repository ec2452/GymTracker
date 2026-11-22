using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Models;
using Sustavzapracenjenapretkauteretani.Web.Services;
using System.Text.Json;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("ai-unos")]
[Authorize(Roles = "Admin,Trainer")]
public class AIController : BaseController
{
    private readonly AIDataEntryService _aiService;
    private readonly VjezbaRepository _vjezbaRepo;
    private readonly KorisnikRepository _korisnikRepo;
    private readonly PlanRepository _planRepo;
    private readonly TreningRepository _treningRepo;
    private readonly MjerenjeRepository _mjerenjeRepo;
    private readonly CiljRepository _ciljRepo;

    public AIController(
        AIDataEntryService aiService,
        VjezbaRepository vjezbaRepo,
        KorisnikRepository korisnikRepo,
        PlanRepository planRepo,
        TreningRepository treningRepo,
        MjerenjeRepository mjerenjeRepo,
        CiljRepository ciljRepo,
        UserManager<AppUser> userManager) : base(userManager)
    {
        _aiService = aiService;
        _vjezbaRepo = vjezbaRepo;
        _korisnikRepo = korisnikRepo;
        _planRepo = planRepo;
        _treningRepo = treningRepo;
        _mjerenjeRepo = mjerenjeRepo;
        _ciljRepo = ciljRepo;
    }

    [HttpGet("", Name = "AIUnos")]
    public IActionResult Index()
    {
        return View(new AIUnosViewModel
        {
            ApiKljucNijePostavljen = !_aiService.IsConfigured
        });
    }

    // ── AJAX: parsira prirodni jezik putem Claude API ──────────────────────────

    [HttpPost("analiziraj")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analiziraj([FromForm] string korisnickiUnos)
    {
        if (string.IsNullOrWhiteSpace(korisnickiUnos))
            return Json(new { uspjeh = false, poruka = "Unos je prazan." });

        if (!_aiService.IsConfigured)
            return Json(new { uspjeh = false, poruka = "Anthropic API ključ nije konfiguriran u appsettings.json." });

        try
        {
            var result = await _aiService.ParseAsync(korisnickiUnos);

            if (result.Tip == "nepoznato")
                return Json(new { uspjeh = false, poruka = result.PotvrdaPoruka });

            return Json(new
            {
                uspjeh = true,
                tip = result.Tip,
                podaciJson = result.Podaci.GetRawText(),
                potvrdaPoruka = result.PotvrdaPoruka,
                originalInput = korisnickiUnos,
                pregled = GradjiPregled(result)
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { uspjeh = false, poruka = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return Json(new { uspjeh = false, poruka = $"Greška API poziva: {ex.Message}" });
        }
        catch (Exception)
        {
            return Json(new { uspjeh = false, poruka = "Neočekivana greška pri analizi. Pokušajte ponovo." });
        }
    }

    // ── Spremi parsirani entitet u bazu ───────────────────────────────────────

    [HttpPost("spremi")]
    [ValidateAntiForgeryToken]
    public IActionResult Spremi(
        [FromForm] string tip,
        [FromForm] string podaciJson,
        [FromForm] string originalInput)
    {
        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            switch (tip.ToLower())
            {
                case "vjezba":
                    return SpremiVjezbu(JsonSerializer.Deserialize<VjezbaAIData>(podaciJson, opts)!);

                case "mjerenje":
                    return SpremiMjerenje(JsonSerializer.Deserialize<MjerenjeAIData>(podaciJson, opts)!);

                case "trening":
                    return SpremiTrening(JsonSerializer.Deserialize<TreningAIData>(podaciJson, opts)!);

                case "plan":
                    return SpremiPlan(JsonSerializer.Deserialize<PlanAIData>(podaciJson, opts)!);

                case "korisnik":
                    return SpremiKorisnika(JsonSerializer.Deserialize<KorisnikAIData>(podaciJson, opts)!);

                case "cilj":
                    return SpremiCilj(JsonSerializer.Deserialize<CiljAIData>(podaciJson, opts)!);

                default:
                    return View("Index", new AIUnosViewModel
                    {
                        KorisnickiUnos = originalInput,
                        GreskaPoruka = $"Nepoznat tip entiteta: {tip}"
                    });
            }
        }
        catch (Exception ex)
        {
            return View("Index", new AIUnosViewModel
            {
                KorisnickiUnos = originalInput,
                GreskaPoruka = $"Greška pri spremanju: {ex.Message}"
            });
        }
    }

    // ── Privatne metode za svaki entitet ──────────────────────────────────────

    private IActionResult SpremiVjezbu(VjezbaAIData d)
    {
        if (string.IsNullOrWhiteSpace(d.Naziv))
            return GreskaPoruka("Naziv vježbe je obavezan.");

        if (_vjezbaRepo.GetAll().Any(v =>
                v.DeletedAt == null &&
                v.Naziv.Equals(d.Naziv.Trim(), StringComparison.OrdinalIgnoreCase)))
            return GreskaPoruka($"Vježba '{d.Naziv}' već postoji.");

        var vjezba = new Vjezba
        {
            Naziv = d.Naziv.Trim(),
            Opis = d.Opis?.Trim(),
            MisicnaSkupina = (MisicnaSkupina)Math.Clamp(d.MisicnaSkupina, 1, 9),
            RazinaTezine = Math.Clamp(d.RazinaTezine, 1, 5),
            PotrebnaOprema = d.PotrebnaOprema
        };
        _vjezbaRepo.Add(vjezba);

        return UspjesnaPoruka($"Vježba \"{vjezba.Naziv}\" uspješno kreirana (ID: {vjezba.Id}).");
    }

    private IActionResult SpremiMjerenje(MjerenjeAIData d)
    {
        var korisnik = NadjiKorisnika(d.KorisnikIme);
        if (korisnik == null) return GreskaPoruka($"Korisnik '{d.KorisnikIme}' nije pronađen u bazi.");

        if (!DateTime.TryParse(d.Datum, out var datum))
            return GreskaPoruka("Neispravan datum mjerenja.");

        var mjerenje = new Mjerenje
        {
            KorisnikId = korisnik.Id,
            Korisnik = korisnik,
            DatumMjerenja = datum,
            Tezina = d.Tezina,
            PostotakMasti = d.PostotakMasti,
            OpsegStruka = d.OpsegStruka
        };
        _mjerenjeRepo.Add(mjerenje);

        return UspjesnaPoruka($"Mjerenje od {datum:dd.MM.yyyy} za {korisnik.Ime} {korisnik.Prezime} uspješno dodano ({d.Tezina} kg).");
    }

    private IActionResult SpremiTrening(TreningAIData d)
    {
        var korisnik = NadjiKorisnika(d.KorisnikIme);
        if (korisnik == null) return GreskaPoruka($"Korisnik '{d.KorisnikIme}' nije pronađen u bazi.");

        if (!DateTime.TryParse(d.Datum, out var datum))
            return GreskaPoruka("Neispravan datum treninga.");

        var trening = new Trening
        {
            KorisnikId = korisnik.Id,
            DatumVrijeme = datum,
            TrajanjeMinuta = Math.Clamp(d.TrajanjeMinuta, 1, 480),
            Vrsta = (VrstaTreninga)Math.Clamp(d.Vrsta, 1, 5),
            Biljeske = d.Biljeske?.Trim(),
            PotroseneKalorije = d.PotroseneKalorije,
            Ocjena = d.Ocjena
        };
        _treningRepo.Add(trening);

        return UspjesnaPoruka($"Trening od {datum:dd.MM.yyyy} za {korisnik.Ime} {korisnik.Prezime} uspješno kreiran ({d.TrajanjeMinuta} min, {(VrstaTreninga)d.Vrsta}).");
    }

    private IActionResult SpremiPlan(PlanAIData d)
    {
        if (string.IsNullOrWhiteSpace(d.Naziv))
            return GreskaPoruka("Naziv plana je obavezan.");

        var plan = new Plan
        {
            Naziv = d.Naziv.Trim(),
            Opis = d.Opis?.Trim(),
            TrajanjeTjedana = Math.Clamp(d.TrajanjeTjedana, 1, 52),
            Razina = (RazinaIskustva)Math.Clamp(d.Razina, 1, 3),
            DatumKreiranja = DateTime.Now
        };
        _planRepo.Add(plan);

        return UspjesnaPoruka($"Plan \"{plan.Naziv}\" uspješno kreiran (ID: {plan.Id}).");
    }

    private IActionResult SpremiKorisnika(KorisnikAIData d)
    {
        if (string.IsNullOrWhiteSpace(d.Email))
            return GreskaPoruka("Email je obavezan.");

        if (_korisnikRepo.GetAll().Any(k =>
                k.Email.Equals(d.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            return GreskaPoruka($"Korisnik s emailom '{d.Email}' već postoji.");

        if (!DateTime.TryParse(d.DatumRodjenja, out var datumRodjenja))
            return GreskaPoruka("Neispravan datum rođenja.");

        var korisnik = new Korisnik
        {
            Ime = d.Ime.Trim(),
            Prezime = d.Prezime.Trim(),
            Email = d.Email.Trim(),
            DatumRodjenja = datumRodjenja,
            DatumRegistracije = DateTime.Now,
            Visina = Math.Clamp(d.Visina, 100, 250),
            Tezina = Math.Clamp(d.Tezina, 30, 300)
        };
        _korisnikRepo.Add(korisnik);

        return UspjesnaPoruka($"Korisnik {korisnik.Ime} {korisnik.Prezime} uspješno registriran (ID: {korisnik.Id}).");
    }

    private IActionResult SpremiCilj(CiljAIData d)
    {
        var korisnik = NadjiKorisnika(d.KorisnikIme);
        if (korisnik == null) return GreskaPoruka($"Korisnik '{d.KorisnikIme}' nije pronađen u bazi.");

        if (!DateTime.TryParse(d.Rok, out var rok))
            return GreskaPoruka("Neispravan datum roka cilja.");

        var cilj = new Cilj
        {
            KorisnikId = korisnik.Id,
            Korisnik = korisnik,
            Tip = (TipCilja)Math.Clamp(d.Tip, 1, 5),
            CiljanaVrijednost = d.CiljanaVrijednost,
            Rok = rok,
            Postignut = false
        };
        _ciljRepo.Add(cilj);

        return UspjesnaPoruka($"Cilj za {korisnik.Ime} {korisnik.Prezime} uspješno postavljen — {(TipCilja)d.Tip}, rok: {rok:dd.MM.yyyy}.");
    }

    // ── Pomoćne metode ────────────────────────────────────────────────────────

    private Korisnik? NadjiKorisnika(string imePrezi)
    {
        if (string.IsNullOrWhiteSpace(imePrezi)) return null;

        var parts = imePrezi.Trim().Split(' ', 2);
        var korisnici = _korisnikRepo.GetAll().Where(k => k.DeletedAt == null).ToList();

        // Traži po punom imenu
        var tocno = korisnici.FirstOrDefault(k =>
            $"{k.Ime} {k.Prezime}".Equals(imePrezi.Trim(), StringComparison.OrdinalIgnoreCase));
        if (tocno != null) return tocno;

        // Traži po imenu ili prezimenu ako nema toucan match
        if (parts.Length >= 1)
        {
            var prviDio = parts[0];
            var kandidati = korisnici.Where(k =>
                k.Ime.Equals(prviDio, StringComparison.OrdinalIgnoreCase) ||
                k.Prezime.Equals(prviDio, StringComparison.OrdinalIgnoreCase)).ToList();
            if (kandidati.Count == 1) return kandidati[0];
        }

        return null;
    }

    private static Dictionary<string, object?> GradjiPregled(AIParseResult result)
    {
        var pregled = new Dictionary<string, object?>();
        try
        {
            foreach (var prop in result.Podaci.EnumerateObject())
            {
                pregled[prop.Name] = prop.Value.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => prop.Value.GetString(),
                    System.Text.Json.JsonValueKind.Number => prop.Value.TryGetDouble(out var d) ? d : (object?)null,
                    System.Text.Json.JsonValueKind.True => true,
                    System.Text.Json.JsonValueKind.False => false,
                    System.Text.Json.JsonValueKind.Null => null,
                    _ => prop.Value.ToString()
                };
            }
        }
        catch { /* ignore */ }
        return pregled;
    }

    private IActionResult UspjesnaPoruka(string poruka) =>
        View("Index", new AIUnosViewModel { UspjesnaPoruka = poruka });

    private IActionResult GreskaPoruka(string poruka) =>
        View("Index", new AIUnosViewModel { GreskaPoruka = poruka });
}
