using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.Json;

namespace Sustavzapracenjenapretkauteretani.PlaywrightTests.Tests;

/// <summary>
/// Playwright E2E scenarij koji pokriva sve API endpointe u 10 koraka.
///
/// PREDUVJET: Aplikacija mora biti pokrenuta na http://localhost:5073
///   dotnet run --project Sustavzapracenjenapretkauteretani.csproj --launch-profile http
///
/// Pokretanje testova:
///   dotnet test Sustavzapracenjenapretkauteretani.PlaywrightTests
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ApiScenarioTest : PlaywrightTest
{
    private const string BaseUrl       = "http://localhost:5073";
    private const string AdminEmail    = "admin@teretana.hr";
    private const string AdminPassword = "Admin@123456";

    private IAPIRequestContext _anonApi = null!;
    private IAPIRequestContext _authApi = null!;

    // ─── SETUP / TEARDOWN ──────────────────────────────────────────────────────

    [SetUp]
    public async Task SetUp()
    {
        _anonApi = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL          = BaseUrl,
            IgnoreHTTPSErrors = true
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _anonApi.DisposeAsync();
        if (_authApi is not null)
            await _authApi.DisposeAsync();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private static int GetInt(JsonElement el, string key)
        => el.GetProperty(key).GetInt32();

    private static string GetStr(JsonElement el, string key)
        => el.GetProperty(key).GetString() ?? string.Empty;

    private async Task<IAPIRequestContext> KreirajAuthKontekst(string token)
    {
        return await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL          = BaseUrl,
            IgnoreHTTPSErrors = true,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Content-Type"]  = "application/json"
            }
        });
    }

    // ─── GLAVNI TEST: 10 KORAKA ─────────────────────────────────────────────────

    [Test]
    [Description("Playwright scenarij s 10 koraka koji pokriva sve API endpointe teretane")]
    public async Task Scenarij_10Koraka_SviApiEndpointi()
    {
        // ======================================================================
        // KORAK 1 — Prijava admina i dobivanje JWT tokena
        //           POST /api/auth/login
        // ======================================================================
        TestContext.WriteLine("=== KORAK 1: Prijava admina ===");

        var loginOdgovor = await _anonApi.PostAsync("/api/auth/login", new APIRequestContextOptions
        {
            DataObject = new { Email = AdminEmail, Password = AdminPassword }
        });

        Assert.That(loginOdgovor.Status, Is.EqualTo(200),
            "Prijava admina mora vratiti HTTP 200 OK");

        var loginTijelo = (await loginOdgovor.JsonAsync())!.Value;
        var jwtToken    = GetStr(loginTijelo, "token");

        Assert.That(jwtToken, Is.Not.Empty,
            "Odgovor na prijavu mora sadržavati JWT token");
        Assert.That(loginTijelo.GetProperty("success").GetBoolean(), Is.True,
            "Polje 'success' mora biti true");

        TestContext.WriteLine($"  JWT token dobiven (duljina: {jwtToken.Length} znakova)");

        // Inicijaliziraj autentificirani kontekst s JWT tokenom
        _authApi = await KreirajAuthKontekst(jwtToken);

        // ======================================================================
        // KORAK 2 — Dohvat liste vježbi
        //           GET /api/vjezbe
        // ======================================================================
        TestContext.WriteLine("=== KORAK 2: Dohvat liste vježbi ===");

        var vjezbeOdgovor = await _authApi.GetAsync("/api/vjezbe");

        Assert.That(vjezbeOdgovor.Status, Is.EqualTo(200),
            "GET /api/vjezbe mora vratiti HTTP 200 OK");

        var vjezbeListaTijelo = (await vjezbeOdgovor.JsonAsync())!.Value;
        Assert.That(vjezbeListaTijelo.ValueKind, Is.EqualTo(JsonValueKind.Array),
            "Odgovor mora biti JSON polje");

        TestContext.WriteLine($"  Pronađeno {vjezbeListaTijelo.GetArrayLength()} vježbi u bazi");

        // ======================================================================
        // KORAK 3 — Kreiranje nove vježbe
        //           POST /api/vjezbe
        // ======================================================================
        TestContext.WriteLine("=== KORAK 3: Kreiranje nove vježbe ===");

        var novaVjezba = new
        {
            naziv          = "Playwright Bench Press",
            opis           = "Vježba kreirana Playwright testom",
            misicnaSkupina = 1,   // Prsa = 1
            razinaTezine   = 3,
            potrebnaOprema = true
        };

        var kreirajVjezbuOdgovor = await _authApi.PostAsync("/api/vjezbe", new APIRequestContextOptions
        {
            DataObject = novaVjezba
        });

        Assert.That(kreirajVjezbuOdgovor.Status, Is.EqualTo(201),
            "POST /api/vjezbe mora vratiti HTTP 201 Created");

        var kreirana = (await kreirajVjezbuOdgovor.JsonAsync())!.Value;
        var vjezbaId = GetInt(kreirana, "id");

        Assert.That(vjezbaId, Is.GreaterThan(0), "Kreirana vježba mora imati ID > 0");
        Assert.That(GetStr(kreirana, "naziv"), Is.EqualTo("Playwright Bench Press"));

        TestContext.WriteLine($"  Kreirana vježba: ID={vjezbaId}, Naziv={GetStr(kreirana, "naziv")}");

        // ======================================================================
        // KORAK 4 — Kreiranje trenažnog plana
        //           POST /api/planovi
        // ======================================================================
        TestContext.WriteLine("=== KORAK 4: Kreiranje trenažnog plana ===");

        var noviPlan = new
        {
            naziv           = "Playwright Plan Snage",
            opis            = "Plan kreiran Playwright E2E testom",
            trajanjeTjedana = 8,
            razina          = 2   // Srednji = 2
        };

        var kreirajPlanOdgovor = await _authApi.PostAsync("/api/planovi", new APIRequestContextOptions
        {
            DataObject = noviPlan
        });

        Assert.That(kreirajPlanOdgovor.Status, Is.EqualTo(201),
            "POST /api/planovi mora vratiti HTTP 201 Created");

        var kreiraniPlan = (await kreirajPlanOdgovor.JsonAsync())!.Value;
        var planId       = GetInt(kreiraniPlan, "id");

        Assert.That(planId, Is.GreaterThan(0), "Kreirani plan mora imati ID > 0");
        Assert.That(GetStr(kreiraniPlan, "naziv"), Is.EqualTo("Playwright Plan Snage"));

        TestContext.WriteLine($"  Kreiran plan: ID={planId}, Naziv={GetStr(kreiraniPlan, "naziv")}");

        // ======================================================================
        // KORAK 5 — Kreiranje korisnika (profila u bazi)
        //           POST /api/korisnici
        // ======================================================================
        TestContext.WriteLine("=== KORAK 5: Kreiranje korisnika ===");

        var noviKorisnik = new
        {
            ime           = "Playwright",
            prezime       = "Tester",
            email         = $"playwright.{Guid.NewGuid():N[..6]}@teretana.hr",
            datumRodjenja = "1995-06-15T00:00:00",
            visina        = 182.0,
            tezina        = 84.5,
            planId        = planId
        };

        var kreirajKorisnikaOdgovor = await _authApi.PostAsync("/api/korisnici", new APIRequestContextOptions
        {
            DataObject = noviKorisnik
        });

        Assert.That(kreirajKorisnikaOdgovor.Status, Is.EqualTo(201),
            "POST /api/korisnici mora vratiti HTTP 201 Created");

        var kreiraniKorisnik = (await kreirajKorisnikaOdgovor.JsonAsync())!.Value;
        var korisnikId       = GetInt(kreiraniKorisnik, "id");

        Assert.That(korisnikId, Is.GreaterThan(0), "Kreirani korisnik mora imati ID > 0");
        Assert.That(GetStr(kreiraniKorisnik, "ime"), Is.EqualTo("Playwright"));

        TestContext.WriteLine($"  Kreiran korisnik: ID={korisnikId}, Ime={GetStr(kreiraniKorisnik, "ime")} {GetStr(kreiraniKorisnik, "prezime")}");

        // ======================================================================
        // KORAK 6 — Kreiranje treninga za korisnika
        //           POST /api/treninzi
        // ======================================================================
        TestContext.WriteLine("=== KORAK 6: Kreiranje treninga ===");

        var noviTrening = new
        {
            datumVrijeme   = DateTime.UtcNow.ToString("o"),
            trajanjeMinuta = 75,
            vrsta          = 1,   // Snaga = 1
            biljeske       = "Playwright E2E test trening",
            potroseneKalorije = 520,
            ocjena         = 9,
            korisnikId     = korisnikId
        };

        var kreirajTreningOdgovor = await _authApi.PostAsync("/api/treninzi", new APIRequestContextOptions
        {
            DataObject = noviTrening
        });

        Assert.That(kreirajTreningOdgovor.Status, Is.EqualTo(201),
            "POST /api/treninzi mora vratiti HTTP 201 Created");

        var kreiraniTrening = (await kreirajTreningOdgovor.JsonAsync())!.Value;
        var treningId       = GetInt(kreiraniTrening, "id");

        Assert.That(treningId, Is.GreaterThan(0), "Kreirani trening mora imati ID > 0");
        Assert.That(kreiraniTrening.GetProperty("trajanjeMinuta").GetInt32(), Is.EqualTo(75));

        TestContext.WriteLine($"  Kreiran trening: ID={treningId}, Trajanje={kreiraniTrening.GetProperty("trajanjeMinuta").GetInt32()} min");

        // ======================================================================
        // KORAK 7 — Dodavanje vježbe u trening
        //           POST /api/trening-vjezbe
        // ======================================================================
        TestContext.WriteLine("=== KORAK 7: Dodavanje vježbe u trening ===");

        var noviTreningVjezba = new
        {
            treningId       = treningId,
            vjezbaId        = vjezbaId,
            brojSerija      = 4,
            brojPonavljanja = 10,
            tezinaKg        = 80.0,
            odmorSekundi    = 90,
            redoslijed      = 1
        };

        var kreirajTVOdgovor = await _authApi.PostAsync("/api/trening-vjezbe", new APIRequestContextOptions
        {
            DataObject = noviTreningVjezba
        });

        Assert.That(kreirajTVOdgovor.Status, Is.EqualTo(201),
            "POST /api/trening-vjezbe mora vratiti HTTP 201 Created");

        var kreiraniTV = (await kreirajTVOdgovor.JsonAsync())!.Value;
        var tvId       = GetInt(kreiraniTV, "id");

        Assert.That(tvId, Is.GreaterThan(0), "Kreirana trening-vježba mora imati ID > 0");
        Assert.That(kreiraniTV.GetProperty("brojSerija").GetInt32(), Is.EqualTo(4));

        TestContext.WriteLine($"  Vježba dodana u trening: TreningVjezbaID={tvId}, Serije={kreiraniTV.GetProperty("brojSerija").GetInt32()}");

        // ======================================================================
        // KORAK 8 — Unos tjelesnih mjerenja korisnika
        //           POST /api/mjerenja
        // ======================================================================
        TestContext.WriteLine("=== KORAK 8: Unos tjelesnih mjerenja ===");

        var novoMjerenje = new
        {
            datumMjerenja = DateTime.UtcNow.Date.ToString("o"),
            tezina        = 84.5,
            postotakMasti = 18.5,
            opsegStruka   = 85.0,
            korisnikId    = korisnikId
        };

        var kreirajMjerenjeOdgovor = await _authApi.PostAsync("/api/mjerenja", new APIRequestContextOptions
        {
            DataObject = novoMjerenje
        });

        Assert.That(kreirajMjerenjeOdgovor.Status, Is.EqualTo(201),
            "POST /api/mjerenja mora vratiti HTTP 201 Created");

        var kreiranMjerenje = (await kreirajMjerenjeOdgovor.JsonAsync())!.Value;
        var mjerenjeId      = GetInt(kreiranMjerenje, "id");

        Assert.That(mjerenjeId, Is.GreaterThan(0), "Kreirano mjerenje mora imati ID > 0");
        Assert.That(kreiranMjerenje.GetProperty("tezina").GetDouble(), Is.EqualTo(84.5));

        TestContext.WriteLine($"  Kreirano mjerenje: ID={mjerenjeId}, Težina={kreiranMjerenje.GetProperty("tezina").GetDouble()} kg");

        // ======================================================================
        // KORAK 9 — Postavljanje fitness cilja za korisnika
        //           POST /api/ciljevi
        // ======================================================================
        TestContext.WriteLine("=== KORAK 9: Postavljanje fitness cilja ===");

        var noviCilj = new
        {
            tip              = 3,   // PovecanjeSnage = 3
            ciljanaVrijednost = 100.0,
            rok              = DateTime.UtcNow.AddMonths(3).ToString("o"),
            postignut        = false,
            korisnikId       = korisnikId
        };

        var kreirajCiljOdgovor = await _authApi.PostAsync("/api/ciljevi", new APIRequestContextOptions
        {
            DataObject = noviCilj
        });

        Assert.That(kreirajCiljOdgovor.Status, Is.EqualTo(201),
            "POST /api/ciljevi mora vratiti HTTP 201 Created");

        var kreiranicilj = (await kreirajCiljOdgovor.JsonAsync())!.Value;
        var ciljId       = GetInt(kreiranicilj, "id");

        Assert.That(ciljId, Is.GreaterThan(0), "Kreirani cilj mora imati ID > 0");
        Assert.That(kreiranicilj.GetProperty("ciljanaVrijednost").GetDouble(), Is.EqualTo(100.0));

        TestContext.WriteLine($"  Kreiran cilj: ID={ciljId}, CiljanaVrijednost={kreiranicilj.GetProperty("ciljanaVrijednost").GetDouble()}");

        // ======================================================================
        // KORAK 10 — Ažuriranje, provjera i čišćenje podataka
        //            PUT /api/planovi/{id} → ažuriranje plana
        //            GET /api/mjerenja?korisnikId={id} → filtriranje mjerenja
        //            GET /api/vjezbe/{id} → dohvat vježbe po ID-u
        //            DELETE /api/vjezbe/{id} → brisanje vježbe (soft delete)
        //            GET /api/vjezbe/{id} → provjera da je vježba obrisana
        // ======================================================================
        TestContext.WriteLine("=== KORAK 10: Ažuriranje, provjera i čišćenje ===");

        // 10a) Ažuriranje plana — PUT /api/planovi/{planId}
        var azuriranPlan = new
        {
            naziv           = "Playwright Plan Snage — AŽURIRANO",
            opis            = "Plan ažuriran Playwright testom",
            trajanjeTjedana = 12,
            razina          = 3   // Napredni = 3
        };

        var azurirajPlanOdgovor = await _authApi.PutAsync($"/api/planovi/{planId}", new APIRequestContextOptions
        {
            DataObject = azuriranPlan
        });

        Assert.That(azurirajPlanOdgovor.Status, Is.EqualTo(204),
            "PUT /api/planovi/{id} mora vratiti HTTP 204 No Content");

        // Provjeri da su promjene spremljene
        var dohvatiPlanOdgovor = await _authApi.GetAsync($"/api/planovi/{planId}");
        var azuriranPlanTijelo = (await dohvatiPlanOdgovor.JsonAsync())!.Value;
        Assert.That(GetStr(azuriranPlanTijelo, "naziv"), Does.Contain("AŽURIRANO"),
            "Naziv plana mora biti ažuriran");
        Assert.That(azuriranPlanTijelo.GetProperty("trajanjeTjedana").GetInt32(), Is.EqualTo(12));

        TestContext.WriteLine($"  Plan ažuriran: Novo trajanje = {azuriranPlanTijelo.GetProperty("trajanjeTjedana").GetInt32()} tjedana");

        // 10b) Filtriranje mjerenja po korisniku — GET /api/mjerenja?korisnikId={id}
        var filtriranaMjerenjaOdgovor = await _authApi.GetAsync($"/api/mjerenja?korisnikId={korisnikId}");
        Assert.That(filtriranaMjerenjaOdgovor.Status, Is.EqualTo(200),
            "GET /api/mjerenja?korisnikId={id} mora vratiti HTTP 200 OK");

        var filtriranaMjerenja = (await filtriranaMjerenjaOdgovor.JsonAsync())!.Value;
        Assert.That(filtriranaMjerenja.GetArrayLength(), Is.GreaterThanOrEqualTo(1),
            "Mora postojati barem jedno mjerenje za kreiranog korisnika");

        TestContext.WriteLine($"  Pronađeno {filtriranaMjerenja.GetArrayLength()} mjerenje(a) za korisnika ID={korisnikId}");

        // 10c) Dohvat vježbe po ID-u — GET /api/vjezbe/{vjezbaId}
        var dohvatiVjezbuOdgovor = await _authApi.GetAsync($"/api/vjezbe/{vjezbaId}");
        Assert.That(dohvatiVjezbuOdgovor.Status, Is.EqualTo(200),
            "GET /api/vjezbe/{id} mora vratiti HTTP 200 dok vježba postoji");

        var dohvacenaVjezba = (await dohvatiVjezbuOdgovor.JsonAsync())!.Value;
        Assert.That(GetStr(dohvacenaVjezba, "naziv"), Is.EqualTo("Playwright Bench Press"));

        TestContext.WriteLine($"  Vježba potvrđena: ID={vjezbaId}, Naziv={GetStr(dohvacenaVjezba, "naziv")}");

        // 10d) Brisanje vježbe — DELETE /api/vjezbe/{vjezbaId}
        var obrisiVjezbuOdgovor = await _authApi.DeleteAsync($"/api/vjezbe/{vjezbaId}");
        Assert.That(obrisiVjezbuOdgovor.Status, Is.EqualTo(204),
            "DELETE /api/vjezbe/{id} mora vratiti HTTP 204 No Content");

        TestContext.WriteLine($"  Vježba ID={vjezbaId} obrisana (soft delete)");

        // 10e) Provjera da obrisana vježba nije dostupna — GET /api/vjezbe/{vjezbaId} → 404
        var provjeriObrisanuOdgovor = await _authApi.GetAsync($"/api/vjezbe/{vjezbaId}");
        Assert.That(provjeriObrisanuOdgovor.Status, Is.EqualTo(404),
            "GET /api/vjezbe/{id} nakon brisanja mora vratiti HTTP 404 Not Found");

        TestContext.WriteLine($"  Potvrđeno: obrisana vježba vraća 404 Not Found");

        TestContext.WriteLine("""

            ╔══════════════════════════════════════════════════════════════╗
            ║   PLAYWRIGHT SCENARIJ S 10 KORAKA — USPJEŠNO ZAVRŠEN!      ║
            ║                                                              ║
            ║   Korak  1: Prijava admina           ✓  POST /api/auth/login        ║
            ║   Korak  2: Lista vježbi             ✓  GET  /api/vjezbe            ║
            ║   Korak  3: Kreiranje vježbe         ✓  POST /api/vjezbe            ║
            ║   Korak  4: Kreiranje plana          ✓  POST /api/planovi           ║
            ║   Korak  5: Kreiranje korisnika      ✓  POST /api/korisnici         ║
            ║   Korak  6: Kreiranje treninga       ✓  POST /api/treninzi          ║
            ║   Korak  7: Vježba u trening         ✓  POST /api/trening-vjezbe    ║
            ║   Korak  8: Unos mjerenja            ✓  POST /api/mjerenja          ║
            ║   Korak  9: Postavljanje cilja       ✓  POST /api/ciljevi           ║
            ║   Korak 10: Ažuriranje + provjera    ✓  PUT/GET/DELETE kombinacija  ║
            ╚══════════════════════════════════════════════════════════════╝
            """);
    }
}
