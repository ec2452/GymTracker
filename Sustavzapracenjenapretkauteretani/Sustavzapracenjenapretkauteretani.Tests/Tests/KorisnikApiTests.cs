using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class KorisnikApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public KorisnikApiTests(TeretanaWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private async Task<Korisnik> SeedKorisnik()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var korisnik    = new Korisnik
        {
            Ime               = "Test",
            Prezime           = "Korisnik",
            Email             = "test." + Guid.NewGuid().ToString("N")[..8] + "@test.com",
            DatumRodjenja     = new DateTime(1990, 5, 15),
            DatumRegistracije = DateTime.Now,
            Visina            = 175,
            Tezina            = 75
        };
        db.Korisnici.Add(korisnik);
        await db.SaveChangesAsync();
        return korisnik;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuKorisnika_Sa200()
    {
        await SeedKorisnik();

        var response = await _client.GetAsync("/api/korisnici");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<KorisnikDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_PretragaPoImenu_VracaOdgovarajuceKorisnike()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var uniq        = Guid.NewGuid().ToString("N")[..6];
        db.Korisnici.Add(new Korisnik
        {
            Ime = "UnikatnoIme" + uniq, Prezime = "Prezime", Email = uniq + "@test.com",
            DatumRodjenja = new DateTime(1985, 1, 1), DatumRegistracije = DateTime.Now,
            Visina = 180, Tezina = 80
        });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/korisnici?pretraga=UnikatnoIme{uniq}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<KorisnikDTO>>();
        Assert.NotNull(lista);
        Assert.Single(lista);
        Assert.Contains(uniq, lista[0].Ime);
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaKorisnika_KadPostoji()
    {
        var korisnik = await SeedKorisnik();

        var response = await _client.GetAsync($"/api/korisnici/{korisnik.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<KorisnikDTO>();
        Assert.NotNull(dto);
        Assert.Equal(korisnik.Id,    dto.Id);
        Assert.Equal(korisnik.Email, dto.Email);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/korisnici/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraKorisnika_I_Vraca201()
    {
        var payload = new KorisnikCreateDTO
        {
            Ime           = "Novi",
            Prezime       = "Korisnik",
            Email         = "novi." + Guid.NewGuid().ToString("N")[..8] + "@test.com",
            DatumRodjenja = new DateTime(1995, 3, 20),
            Visina        = 178,
            Tezina        = 72
        };

        var response = await _client.PostAsJsonAsync("/api/korisnici", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<KorisnikDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(payload.Email, dto.Email);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // Visina = 50 krši [Range(100, 250)]
        var payload = new { Ime = "Loš", Prezime = "Korisnik", Email = "los@test.com", Visina = 50, Tezina = 70, DatumRodjenja = "1990-01-01" };

        var response = await _client.PostAsJsonAsync("/api/korisnici", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraKorisnika_I_Vraca204()
    {
        var korisnik = await SeedKorisnik();
        var payload  = new KorisnikCreateDTO
        {
            Ime           = "IzmijenjIme",
            Prezime       = "IzmijenjPrezime",
            Email         = "azuriran." + Guid.NewGuid().ToString("N")[..6] + "@test.com",
            DatumRodjenja = new DateTime(1988, 7, 7),
            Visina        = 180,
            Tezina        = 80
        };

        var response = await _client.PutAsJsonAsync($"/api/korisnici/{korisnik.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/korisnici/{korisnik.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<KorisnikDTO>();
        Assert.Equal(payload.Ime, dto!.Ime);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new KorisnikCreateDTO
        {
            Ime = "X", Prezime = "X", Email = "x@test.com",
            DatumRodjenja = new DateTime(1990, 1, 1), Visina = 170, Tezina = 65
        };

        var response = await _client.PutAsJsonAsync("/api/korisnici/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseKorisnika_I_Vraca204()
    {
        var korisnik = await SeedKorisnik();

        var deleteResponse = await _client.DeleteAsync($"/api/korisnici/{korisnik.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/korisnici/{korisnik.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/korisnici/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
