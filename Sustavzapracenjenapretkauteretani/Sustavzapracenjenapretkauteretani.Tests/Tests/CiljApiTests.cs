using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class CiljApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public CiljApiTests(TeretanaWebApplicationFactory factory)
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
            Ime = "C", Prezime = "K", Email = Guid.NewGuid().ToString("N")[..8] + "@cilj.com",
            DatumRodjenja = new DateTime(1993, 3, 3), DatumRegistracije = DateTime.Now,
            Visina = 170, Tezina = 65
        };
        db.Korisnici.Add(k);
        await db.SaveChangesAsync();
        return k;
    }

    private async Task<Cilj> SeedCilj(int korisnikId)
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var c = new Cilj
        {
            Tip              = TipCilja.Mrsavljenje,
            CiljanaVrijednost = 60,
            Rok              = DateTime.Today.AddMonths(3),
            Postignut        = false,
            KorisnikId       = korisnikId
        };
        db.Ciljevi.Add(c);
        await db.SaveChangesAsync();
        return c;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuCiljeva_Sa200()
    {
        var k = await SeedKorisnik();
        await SeedCilj(k.Id);

        var response = await _client.GetAsync("/api/ciljevi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<CiljDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoPostignut_VracaTocneCiljeve()
    {
        var k = await SeedKorisnik();
        using var scope = _factory.Services.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        db.Ciljevi.Add(new Cilj { Tip = TipCilja.Mrsavljenje,    CiljanaVrijednost = 55, Rok = DateTime.Today.AddMonths(1), Postignut = true,  KorisnikId = k.Id });
        db.Ciljevi.Add(new Cilj { Tip = TipCilja.DobivanjeMase, CiljanaVrijednost = 80, Rok = DateTime.Today.AddMonths(2), Postignut = false, KorisnikId = k.Id });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/ciljevi?postignut=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<CiljDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, c => Assert.True(c.Postignut));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaCilj_KadPostoji()
    {
        var k    = await SeedKorisnik();
        var cilj = await SeedCilj(k.Id);

        var response = await _client.GetAsync($"/api/ciljevi/{cilj.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<CiljDTO>();
        Assert.NotNull(dto);
        Assert.Equal(cilj.Id, dto.Id);
        Assert.Equal(k.Id,    dto.KorisnikId);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/ciljevi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraCilj_I_Vraca201()
    {
        var k = await SeedKorisnik();
        var payload = new CiljCreateDTO
        {
            Tip               = TipCilja.PovecanjeSnage,
            CiljanaVrijednost = 100,
            Rok               = DateTime.Today.AddMonths(6),
            Postignut         = false,
            KorisnikId        = k.Id
        };

        var response = await _client.PostAsJsonAsync("/api/ciljevi", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<CiljDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal("PovecanjeSnage", dto.Tip);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // CiljanaVrijednost = 0 krši [Range(0.1, 10000)]
        var payload = new { Tip = 1, CiljanaVrijednost = 0.0, Rok = DateTime.Today.AddMonths(1), KorisnikId = 1 };

        var response = await _client.PostAsJsonAsync("/api/ciljevi", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraCilj_I_Vraca204()
    {
        var k    = await SeedKorisnik();
        var cilj = await SeedCilj(k.Id);
        var payload = new CiljCreateDTO
        {
            Tip               = TipCilja.Izdrzljivost,
            CiljanaVrijednost = 75,
            Rok               = DateTime.Today.AddMonths(4),
            Postignut         = true,
            KorisnikId        = k.Id
        };

        var response = await _client.PutAsJsonAsync($"/api/ciljevi/{cilj.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/ciljevi/{cilj.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<CiljDTO>();
        Assert.Equal("Izdrzljivost", dto!.Tip);
        Assert.True(dto.Postignut);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new CiljCreateDTO
        {
            Tip = TipCilja.Mrsavljenje, CiljanaVrijednost = 60, Rok = DateTime.Today.AddMonths(1), KorisnikId = 1
        };

        var response = await _client.PutAsJsonAsync("/api/ciljevi/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseCilj_I_Vraca204()
    {
        var k    = await SeedKorisnik();
        var cilj = await SeedCilj(k.Id);

        var deleteResponse = await _client.DeleteAsync($"/api/ciljevi/{cilj.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/ciljevi/{cilj.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/ciljevi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
