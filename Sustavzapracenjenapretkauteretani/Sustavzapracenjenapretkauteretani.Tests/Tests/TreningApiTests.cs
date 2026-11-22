using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class TreningApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public TreningApiTests(TeretanaWebApplicationFactory factory)
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
            Ime = "K", Prezime = "T", Email = Guid.NewGuid().ToString("N")[..8] + "@trening.com",
            DatumRodjenja = new DateTime(1990, 1, 1), DatumRegistracije = DateTime.Now,
            Visina = 175, Tezina = 75
        };
        db.Korisnici.Add(k);
        await db.SaveChangesAsync();
        return k;
    }

    private async Task<Trening> SeedTrening(int korisnikId)
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var t = new Trening
        {
            DatumVrijeme   = DateTime.Now,
            TrajanjeMinuta = 60,
            Vrsta          = VrstaTreninga.Snaga,
            KorisnikId     = korisnikId
        };
        db.Treninzi.Add(t);
        await db.SaveChangesAsync();
        return t;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuTreninga_Sa200()
    {
        var k = await SeedKorisnik();
        await SeedTrening(k.Id);

        var response = await _client.GetAsync("/api/treninzi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<TreningDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoBrojuKorisnika_VracaTocnoTreninge()
    {
        var k = await SeedKorisnik();
        await SeedTrening(k.Id);

        var response = await _client.GetAsync($"/api/treninzi?korisnikId={k.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<TreningDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, t => Assert.Equal(k.Id, t.KorisnikId));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaTrening_KadPostoji()
    {
        var k       = await SeedKorisnik();
        var trening = await SeedTrening(k.Id);

        var response = await _client.GetAsync($"/api/treninzi/{trening.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TreningDTO>();
        Assert.NotNull(dto);
        Assert.Equal(trening.Id, dto.Id);
        Assert.Equal(k.Id,       dto.KorisnikId);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/treninzi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraTrening_I_Vraca201()
    {
        var k = await SeedKorisnik();
        var payload = new TreningCreateDTO
        {
            DatumVrijeme   = DateTime.Now,
            TrajanjeMinuta = 45,
            Vrsta          = VrstaTreninga.Kardio,
            KorisnikId     = k.Id,
            Ocjena         = 8
        };

        var response = await _client.PostAsJsonAsync("/api/treninzi", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TreningDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(k.Id, dto.KorisnikId);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // TrajanjeMinuta = 0 krši [Range(1, 480)]
        var payload = new { DatumVrijeme = DateTime.Now, TrajanjeMinuta = 0, Vrsta = 1, KorisnikId = 1 };

        var response = await _client.PostAsJsonAsync("/api/treninzi", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraTrening_I_Vraca204()
    {
        var k       = await SeedKorisnik();
        var trening = await SeedTrening(k.Id);
        var payload = new TreningCreateDTO
        {
            DatumVrijeme   = DateTime.Now,
            TrajanjeMinuta = 90,
            Vrsta          = VrstaTreninga.HIIT,
            KorisnikId     = k.Id,
            Ocjena         = 9
        };

        var response = await _client.PutAsJsonAsync($"/api/treninzi/{trening.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/treninzi/{trening.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<TreningDTO>();
        Assert.Equal(90,            dto!.TrajanjeMinuta);
        Assert.Equal("HIIT",        dto.Vrsta);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new TreningCreateDTO
        {
            DatumVrijeme = DateTime.Now, TrajanjeMinuta = 30, Vrsta = VrstaTreninga.Snaga, KorisnikId = 1
        };

        var response = await _client.PutAsJsonAsync("/api/treninzi/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseTrening_I_Vraca204()
    {
        var k       = await SeedKorisnik();
        var trening = await SeedTrening(k.Id);

        var deleteResponse = await _client.DeleteAsync($"/api/treninzi/{trening.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/treninzi/{trening.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/treninzi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
