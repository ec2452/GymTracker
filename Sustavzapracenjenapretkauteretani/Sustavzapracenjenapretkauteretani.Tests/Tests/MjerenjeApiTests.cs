using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class MjerenjeApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public MjerenjeApiTests(TeretanaWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private async Task<Korisnik> SeedKorisnik()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var k = new Korisnik
        {
            Ime = "M", Prezime = "K", Email = Guid.NewGuid().ToString("N")[..8] + "@mj.com",
            DatumRodjenja = new DateTime(1992, 6, 10), DatumRegistracije = DateTime.Now,
            Visina = 168, Tezina = 62
        };
        db.Korisnici.Add(k);
        await db.SaveChangesAsync();
        return k;
    }

    private async Task<Mjerenje> SeedMjerenje(int korisnikId)
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var m = new Mjerenje
        {
            DatumMjerenja  = DateTime.Today,
            Tezina         = 70,
            PostotakMasti  = 20,
            OpsegStruka    = 80,
            KorisnikId     = korisnikId
        };
        db.Mjerenja.Add(m);
        await db.SaveChangesAsync();
        return m;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuMjerenja_Sa200()
    {
        var k = await SeedKorisnik();
        await SeedMjerenje(k.Id);

        var response = await _client.GetAsync("/api/mjerenja");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<MjerenjeDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoDatumuOd_VracaTocnaMjerenja()
    {
        var k  = await SeedKorisnik();
        using var scope = _factory.Services.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        db.Mjerenja.Add(new Mjerenje { DatumMjerenja = new DateTime(2025, 1, 1), Tezina = 65, KorisnikId = k.Id });
        db.Mjerenja.Add(new Mjerenje { DatumMjerenja = new DateTime(2025, 6, 1), Tezina = 63, KorisnikId = k.Id });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/mjerenja?od=2025-03-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<MjerenjeDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, m => Assert.True(m.DatumMjerenja >= new DateTime(2025, 3, 1)));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaMjerenje_KadPostoji()
    {
        var k        = await SeedKorisnik();
        var mjerenje = await SeedMjerenje(k.Id);

        var response = await _client.GetAsync($"/api/mjerenja/{mjerenje.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<MjerenjeDTO>();
        Assert.NotNull(dto);
        Assert.Equal(mjerenje.Id, dto.Id);
        Assert.Equal(k.Id,        dto.KorisnikId);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/mjerenja/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraMjerenje_I_Vraca201()
    {
        var k = await SeedKorisnik();
        var payload = new MjerenjeCreateDTO
        {
            DatumMjerenja = DateTime.Today,
            Tezina        = 68.5,
            PostotakMasti = 18.5,
            OpsegStruka   = 78,
            KorisnikId    = k.Id
        };

        var response = await _client.PostAsJsonAsync("/api/mjerenja", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<MjerenjeDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(k.Id, dto.KorisnikId);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // Tezina = 0 krši [Range(30, 300)]
        var payload = new { DatumMjerenja = DateTime.Today, Tezina = 0.0, KorisnikId = 1 };

        var response = await _client.PostAsJsonAsync("/api/mjerenja", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraMjerenje_I_Vraca204()
    {
        var k        = await SeedKorisnik();
        var mjerenje = await SeedMjerenje(k.Id);
        var payload  = new MjerenjeCreateDTO
        {
            DatumMjerenja = DateTime.Today,
            Tezina        = 69,
            PostotakMasti = 19,
            OpsegStruka   = 82,
            KorisnikId    = k.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/mjerenja/{mjerenje.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/mjerenja/{mjerenje.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<MjerenjeDTO>();
        Assert.Equal(69, dto!.Tezina);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new MjerenjeCreateDTO { DatumMjerenja = DateTime.Today, Tezina = 65, KorisnikId = 1 };

        var response = await _client.PutAsJsonAsync("/api/mjerenja/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseMjerenje_I_Vraca204()
    {
        var k        = await SeedKorisnik();
        var mjerenje = await SeedMjerenje(k.Id);

        var deleteResponse = await _client.DeleteAsync($"/api/mjerenja/{mjerenje.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/mjerenja/{mjerenje.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/mjerenja/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
