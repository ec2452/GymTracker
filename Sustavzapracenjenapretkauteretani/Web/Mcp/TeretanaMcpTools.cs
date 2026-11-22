using ModelContextProtocol.Server;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using System.ComponentModel;
using System.Text.Json;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Mcp;

[McpServerToolType]
public sealed class TeretanaMcpTools(
    VjezbaRepository vjezbaRepo,
    KorisnikRepository korisnikRepo,
    PlanRepository planRepo,
    TreningRepository treningRepo,
    MjerenjeRepository mjerenjeRepo,
    CiljRepository ciljRepo)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    // ── VJEZBE ─────────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati sve vježbe iz kataloga GymTracker aplikacije. " +
        "Opcija pretrage po nazivu. MisicnaSkupina: Prsa/Leda/Ramena/Biceps/Triceps/Noge/Trbuh/Kardio/CijeloTijelo.")]
    public string DohvatiVjezbe(
        [Description("Opcionalni pojam za filtriranje po nazivu (ostavi prazno za sve)")] string? pretraga = null)
    {
        var vjezbe = vjezbaRepo.GetAll()
            .Where(v => v.DeletedAt == null)
            .Where(v => pretraga == null || v.Naziv.Contains(pretraga, StringComparison.OrdinalIgnoreCase))
            .Select(v => new
            {
                v.Id,
                v.Naziv,
                v.Opis,
                MisicnaSkupina = v.MisicnaSkupina.ToString(),
                v.RazinaTezine,
                v.PotrebnaOprema,
                v.VideoUrl
            })
            .ToList();

        return vjezbe.Count == 0
            ? "Nema vježbi u bazi."
            : JsonSerializer.Serialize(vjezbe, _json);
    }

    [McpServerTool, Description("Dohvati detalje jedne vježbe po ID-u.")]
    public string DohvatiVjezbu(
        [Description("Numerički ID vježbe")] int id)
    {
        var v = vjezbaRepo.GetById(id);
        if (v is null || v.DeletedAt != null)
            return $"Vježba s ID-om {id} nije pronađena.";

        return JsonSerializer.Serialize(new
        {
            v.Id, v.Naziv, v.Opis,
            MisicnaSkupina = v.MisicnaSkupina.ToString(),
            v.RazinaTezine, v.PotrebnaOprema, v.VideoUrl
        }, _json);
    }

    [McpServerTool, Description(
        "Kreiraj novu vježbu u katalog. " +
        "misicnaSkupina: 1=Prsa, 2=Leda, 3=Ramena, 4=Biceps, 5=Triceps, 6=Noge, 7=Trbuh, 8=Kardio, 9=CijeloTijelo. " +
        "razinaTezine: 1 (najlakše) do 5 (najteže).")]
    public string KreirajVjezbu(
        [Description("Naziv vježbe (max 100 znakova)")] string naziv,
        [Description("Mišićna skupina (1-9)")] int misicnaSkupina,
        [Description("Razina težine (1-5)")] int razinaTezine,
        [Description("Opis vježbe (opcionalno)")] string? opis = null,
        [Description("Je li potrebna oprema?")] bool potrebnaOprema = false,
        [Description("URL videa za demonstraciju (opcionalno)")] string? videoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(naziv))
            return "Greška: Naziv je obavezan.";
        if (misicnaSkupina < 1 || misicnaSkupina > 9)
            return "Greška: misicnaSkupina mora biti broj 1-9.";
        if (razinaTezine < 1 || razinaTezine > 5)
            return "Greška: razinaTezine mora biti broj 1-5.";

        if (vjezbaRepo.GetAll().Any(v =>
                v.DeletedAt == null &&
                v.Naziv.Equals(naziv.Trim(), StringComparison.OrdinalIgnoreCase)))
            return $"Greška: Vježba s nazivom '{naziv}' već postoji.";

        var vjezba = new Vjezba
        {
            Naziv = naziv.Trim(),
            Opis = opis?.Trim(),
            MisicnaSkupina = (MisicnaSkupina)misicnaSkupina,
            RazinaTezine = razinaTezine,
            PotrebnaOprema = potrebnaOprema,
            VideoUrl = videoUrl
        };
        vjezbaRepo.Add(vjezba);

        return JsonSerializer.Serialize(new
        {
            uspjeh = true,
            id = vjezba.Id,
            poruka = $"Vježba '{vjezba.Naziv}' uspješno kreirana s ID-om {vjezba.Id}."
        }, _json);
    }

    // ── PLANOVI ────────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati sve trenažne planove iz baze. " +
        "Razina iskustva: Pocetnik, Srednji ili Napredni.")]
    public string DohvatiPlanove()
    {
        var planovi = planRepo.GetAll()
            .Where(p => p.DeletedAt == null)
            .Select(p => new
            {
                p.Id, p.Naziv, p.Opis, p.TrajanjeTjedana,
                Razina = p.Razina.ToString(),
                p.DatumKreiranja,
                BrojVjezbi = p.Vjezbe.Count,
                BrojKorisnika = p.Korisnici.Count
            })
            .ToList();

        return planovi.Count == 0
            ? "Nema planova u bazi."
            : JsonSerializer.Serialize(planovi, _json);
    }

    [McpServerTool, Description("Dohvati detalje jednog plana po ID-u s popisom vježbi i korisnika koji ga prate.")]
    public string DohvatiPlan(
        [Description("Numerički ID plana")] int id)
    {
        var p = planRepo.GetById(id);
        if (p is null || p.DeletedAt != null)
            return $"Plan s ID-om {id} nije pronađen.";

        return JsonSerializer.Serialize(new
        {
            p.Id, p.Naziv, p.Opis, p.TrajanjeTjedana,
            Razina = p.Razina.ToString(), p.DatumKreiranja,
            Vjezbe = p.Vjezbe.Select(v => new { v.Id, v.Naziv, MisicnaSkupina = v.MisicnaSkupina.ToString() }),
            Korisnici = p.Korisnici.Select(k => new { k.Id, k.Ime, k.Prezime, k.Email })
        }, _json);
    }

    [McpServerTool, Description(
        "Kreiraj novi trenažni plan. " +
        "razina: 1=Pocetnik, 2=Srednji, 3=Napredni.")]
    public string KreirajPlan(
        [Description("Naziv plana (max 100 znakova)")] string naziv,
        [Description("Trajanje plana u tjednima (1-52)")] int trajanjeTjedana,
        [Description("Razina iskustva: 1=Pocetnik, 2=Srednji, 3=Napredni")] int razina,
        [Description("Opis plana (opcionalno)")] string? opis = null)
    {
        if (string.IsNullOrWhiteSpace(naziv))
            return "Greška: Naziv je obavezan.";
        if (trajanjeTjedana < 1 || trajanjeTjedana > 52)
            return "Greška: trajanjeTjedana mora biti 1-52.";
        if (razina < 1 || razina > 3)
            return "Greška: razina mora biti 1 (Pocetnik), 2 (Srednji) ili 3 (Napredni).";

        var plan = new Plan
        {
            Naziv = naziv.Trim(),
            Opis = opis?.Trim(),
            TrajanjeTjedana = trajanjeTjedana,
            Razina = (RazinaIskustva)razina,
            DatumKreiranja = DateTime.Now
        };
        planRepo.Add(plan);

        return JsonSerializer.Serialize(new
        {
            uspjeh = true,
            id = plan.Id,
            poruka = $"Plan '{plan.Naziv}' uspješno kreiran s ID-om {plan.Id}."
        }, _json);
    }

    // ── KORISNICI ──────────────────────────────────────────────────────────────

    [McpServerTool, Description("Dohvati sve registrirane korisnike teretane s osnovnim podacima.")]
    public string DohvatiKorisnike()
    {
        var korisnici = korisnikRepo.GetAll()
            .Where(k => k.DeletedAt == null)
            .Select(k => new
            {
                k.Id, k.Ime, k.Prezime, k.Email,
                k.DatumRodjenja, k.Visina, k.Tezina,
                PlanId = k.PlanId,
                Plan = k.TrenutniPlan?.Naziv
            })
            .ToList();

        return korisnici.Count == 0
            ? "Nema korisnika u bazi."
            : JsonSerializer.Serialize(korisnici, _json);
    }

    [McpServerTool, Description(
        "Dohvati detalje korisnika po ID-u — uključuje zadnje mjerenje, " +
        "broj ciljeva, treninga i plan koji korisnik prati.")]
    public string DohvatiKorisnika(
        [Description("Numerički ID korisnika")] int id)
    {
        var k = korisnikRepo.GetById(id);
        if (k is null || k.DeletedAt != null)
            return $"Korisnik s ID-om {id} nije pronađen.";

        var zadnjeMjerenje = k.Mjerenja
            .OrderByDescending(m => m.DatumMjerenja)
            .FirstOrDefault();

        return JsonSerializer.Serialize(new
        {
            k.Id, k.Ime, k.Prezime, k.Email,
            k.DatumRodjenja, k.DatumRegistracije,
            k.Visina, k.Tezina,
            Plan = k.TrenutniPlan?.Naziv,
            BrojTreninga = k.Treninzi.Count(t => t.DeletedAt == null),
            BrojMjerenja = k.Mjerenja.Count,
            BrojCiljeva = k.Ciljevi.Count(c => c.DeletedAt == null),
            ZadnjeMjerenje = zadnjeMjerenje == null ? null : (object)new
            {
                zadnjeMjerenje.DatumMjerenja,
                zadnjeMjerenje.Tezina,
                zadnjeMjerenje.PostotakMasti,
                zadnjeMjerenje.OpsegStruka
            }
        }, _json);
    }

    // ── TRENINZI ───────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati treninge (zadnjih 30). Opcija filtriranja po korisniku. " +
        "VrstaTreninga: Snaga, Kardio, HIIT, Funkcionalni, Mobility.")]
    public string DohvatiTreninge(
        [Description("ID korisnika za filtriranje (0 = svi korisnici)")] int korisnikId = 0)
    {
        var treninzi = treningRepo.GetAll()
            .Where(t => t.DeletedAt == null)
            .Where(t => korisnikId == 0 || t.KorisnikId == korisnikId)
            .OrderByDescending(t => t.DatumVrijeme)
            .Take(30)
            .Select(t => new
            {
                t.Id, t.DatumVrijeme, t.TrajanjeMinuta,
                Vrsta = t.Vrsta.ToString(),
                t.KorisnikId,
                Korisnik = t.Korisnik != null ? $"{t.Korisnik.Ime} {t.Korisnik.Prezime}" : null,
                t.Ocjena, t.PotroseneKalorije,
                BrojVjezbi = t.TreningVjezbe.Count(tv => tv.DeletedAt == null)
            })
            .ToList();

        return treninzi.Count == 0
            ? "Nema treninga."
            : JsonSerializer.Serialize(treninzi, _json);
    }

    [McpServerTool, Description(
        "Kreiraj novi trening za korisnika. " +
        "vrsta: 1=Snaga, 2=Kardio, 3=HIIT, 4=Funkcionalni, 5=Mobility.")]
    public string KreirajTrening(
        [Description("ID korisnika")] int korisnikId,
        [Description("Datum i vrijeme treninga (format: yyyy-MM-dd HH:mm ili yyyy-MM-dd)")] string datumVrijeme,
        [Description("Trajanje u minutama (1-480)")] int trajanjeMinuta,
        [Description("Vrsta treninga: 1=Snaga, 2=Kardio, 3=HIIT, 4=Funkcionalni, 5=Mobility")] int vrsta,
        [Description("Bilješke o treningu (opcionalno)")] string? biljeske = null,
        [Description("Potrošene kalorije (opcionalno, 0-5000)")] int? potroseneKalorije = null,
        [Description("Ocjena treninga 1-10 (opcionalno)")] int? ocjena = null)
    {
        var korisnik = korisnikRepo.GetById(korisnikId);
        if (korisnik is null || korisnik.DeletedAt != null)
            return $"Greška: Korisnik s ID-om {korisnikId} nije pronađen.";
        if (!DateTime.TryParse(datumVrijeme, out var dt))
            return "Greška: Neispravan format datuma. Koristite yyyy-MM-dd HH:mm.";
        if (dt > DateTime.Now.AddDays(1))
            return "Greška: Datum treninga ne smije biti više od 1 dan u budućnosti.";
        if (trajanjeMinuta < 1 || trajanjeMinuta > 480)
            return "Greška: trajanjeMinuta mora biti 1-480.";
        if (vrsta < 1 || vrsta > 5)
            return "Greška: vrsta mora biti 1-5.";

        var trening = new Trening
        {
            KorisnikId = korisnikId,
            DatumVrijeme = dt,
            TrajanjeMinuta = trajanjeMinuta,
            Vrsta = (VrstaTreninga)vrsta,
            Biljeske = biljeske?.Trim(),
            PotroseneKalorije = potroseneKalorije,
            Ocjena = ocjena
        };
        treningRepo.Add(trening);

        return JsonSerializer.Serialize(new
        {
            uspjeh = true,
            id = trening.Id,
            poruka = $"Trening od {dt:dd.MM.yyyy} za korisnika {korisnik.Ime} {korisnik.Prezime} kreiran s ID-om {trening.Id}."
        }, _json);
    }

    // ── MJERENJA ───────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati sva tjelesna mjerenja za određenog korisnika, " +
        "sortirana od najnovijeg prema najstarijem.")]
    public string DohvatiMjerenja(
        [Description("ID korisnika")] int korisnikId)
    {
        var korisnik = korisnikRepo.GetById(korisnikId);
        if (korisnik is null)
            return $"Korisnik s ID-om {korisnikId} nije pronađen.";

        var mjerenja = mjerenjeRepo.GetAll()
            .Where(m => m.KorisnikId == korisnikId)
            .OrderByDescending(m => m.DatumMjerenja)
            .Select(m => new
            {
                m.Id, m.DatumMjerenja, m.Tezina,
                m.PostotakMasti, m.OpsegStruka
            })
            .ToList();

        return mjerenja.Count == 0
            ? $"Korisnik {korisnik.Ime} {korisnik.Prezime} nema mjerenja."
            : JsonSerializer.Serialize(mjerenja, _json);
    }

    [McpServerTool, Description("Dodaj novo tjelesno mjerenje za korisnika.")]
    public string KreirajMjerenje(
        [Description("ID korisnika")] int korisnikId,
        [Description("Datum mjerenja (format: yyyy-MM-dd)")] string datum,
        [Description("Tjelesna težina u kg (30-300)")] double tezina,
        [Description("Postotak tjelesne masti u % (3-60, opcionalno)")] double? postotakMasti = null,
        [Description("Opseg struka u cm (40-200, opcionalno)")] double? opsegStruka = null)
    {
        var korisnik = korisnikRepo.GetById(korisnikId);
        if (korisnik is null || korisnik.DeletedAt != null)
            return $"Greška: Korisnik s ID-om {korisnikId} nije pronađen.";
        if (!DateTime.TryParse(datum, out var datumMjerenja))
            return "Greška: Neispravan format datuma. Koristite yyyy-MM-dd.";
        if (datumMjerenja > DateTime.Today)
            return "Greška: Datum mjerenja ne smije biti u budućnosti.";
        if (datumMjerenja < korisnik.DatumRodjenja)
            return "Greška: Datum mjerenja ne može biti prije datuma rođenja korisnika.";
        if (tezina < 30 || tezina > 300)
            return "Greška: Težina mora biti između 30 i 300 kg.";

        var mjerenje = new Mjerenje
        {
            KorisnikId = korisnikId,
            Korisnik = korisnik,
            DatumMjerenja = datumMjerenja,
            Tezina = tezina,
            PostotakMasti = postotakMasti,
            OpsegStruka = opsegStruka
        };
        mjerenjeRepo.Add(mjerenje);

        return JsonSerializer.Serialize(new
        {
            uspjeh = true,
            id = mjerenje.Id,
            poruka = $"Mjerenje od {datumMjerenja:dd.MM.yyyy} ({tezina} kg) za {korisnik.Ime} {korisnik.Prezime} uspješno dodano."
        }, _json);
    }

    // ── CILJEVI ────────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati aktivne ciljeve korisnika. " +
        "Tipovi cilja: Mrsavljenje, DobivanjeMase, PovecanjeSnage, Izdrzljivost, Fleksibilnost.")]
    public string DohvatiCiljeve(
        [Description("ID korisnika")] int korisnikId)
    {
        var korisnik = korisnikRepo.GetById(korisnikId);
        if (korisnik is null)
            return $"Korisnik s ID-om {korisnikId} nije pronađen.";

        var ciljevi = ciljRepo.GetAll()
            .Where(c => c.KorisnikId == korisnikId && c.DeletedAt == null)
            .Select(c => new
            {
                c.Id,
                Tip = c.Tip.ToString(),
                c.CiljanaVrijednost,
                c.Rok,
                c.Postignut
            })
            .ToList();

        return ciljevi.Count == 0
            ? $"Korisnik {korisnik.Ime} {korisnik.Prezime} nema aktivnih ciljeva."
            : JsonSerializer.Serialize(ciljevi, _json);
    }

    // ── STATISTIKE ─────────────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Dohvati opće statistike GymTracker sustava — " +
        "ukupan broj vježbi, planova, korisnika, treninga, mjerenja i ciljeva u bazi.")]
    public string DohvatiStatistike()
    {
        return JsonSerializer.Serialize(new
        {
            vjezbe = vjezbaRepo.GetAll().Count(v => v.DeletedAt == null),
            planovi = planRepo.GetAll().Count(p => p.DeletedAt == null),
            korisnici = korisnikRepo.GetAll().Count(k => k.DeletedAt == null),
            treninzi = treningRepo.GetAll().Count(t => t.DeletedAt == null),
            mjerenja = mjerenjeRepo.GetAll().Count,
            ciljevi = ciljRepo.GetAll().Count(c => c.DeletedAt == null)
        }, _json);
    }

    // ── GLOBALNA PRETRAGA ──────────────────────────────────────────────────────

    [McpServerTool, Description(
        "Pretraži sve podatke u GymTracker bazi — vježbe, planove i korisnike. " +
        "Vraća do 5 rezultata po kategoriji.")]
    public string Pretrazi(
        [Description("Pojam za pretragu (minimalno 2 znaka)")] string upit)
    {
        if (string.IsNullOrWhiteSpace(upit) || upit.Length < 2)
            return "Unesite najmanje 2 znaka za pretragu.";

        var vjezbe = vjezbaRepo.GetAll()
            .Where(v => v.DeletedAt == null &&
                        (v.Naziv.Contains(upit, StringComparison.OrdinalIgnoreCase) ||
                         (v.Opis != null && v.Opis.Contains(upit, StringComparison.OrdinalIgnoreCase))))
            .Take(5)
            .Select(v => new { tip = "vjezba", v.Id, naziv = v.Naziv, info = v.MisicnaSkupina.ToString() });

        var planovi = planRepo.GetAll()
            .Where(p => p.DeletedAt == null &&
                        (p.Naziv.Contains(upit, StringComparison.OrdinalIgnoreCase) ||
                         (p.Opis != null && p.Opis.Contains(upit, StringComparison.OrdinalIgnoreCase))))
            .Take(5)
            .Select(p => new { tip = "plan", p.Id, naziv = p.Naziv, info = $"{p.TrajanjeTjedana} tjedana, {p.Razina}" });

        var korisnici = korisnikRepo.GetAll()
            .Where(k => k.DeletedAt == null &&
                        (k.Ime.Contains(upit, StringComparison.OrdinalIgnoreCase) ||
                         k.Prezime.Contains(upit, StringComparison.OrdinalIgnoreCase) ||
                         k.Email.Contains(upit, StringComparison.OrdinalIgnoreCase)))
            .Take(5)
            .Select(k => new { tip = "korisnik", k.Id, naziv = $"{k.Ime} {k.Prezime}", info = k.Email });

        var rezultati = vjezbe.Cast<object>()
            .Concat(planovi.Cast<object>())
            .Concat(korisnici.Cast<object>())
            .ToList();

        return rezultati.Count == 0
            ? $"Nema rezultata za pojam '{upit}'."
            : JsonSerializer.Serialize(new
            {
                upit,
                ukupnoRezultata = rezultati.Count,
                rezultati
            }, _json);
    }
}
