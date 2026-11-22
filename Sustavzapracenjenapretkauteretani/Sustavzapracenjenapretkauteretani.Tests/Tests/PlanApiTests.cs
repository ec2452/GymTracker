using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class PlanApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public PlanApiTests(TeretanaWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private async Task<Plan> SeedPlan()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var plan = new Plan
        {
            Naziv           = "Test Plan " + Guid.NewGuid().ToString("N")[..6],
            Opis            = "Testni plan treninga",
            TrajanjeTjedana = 8,
            Razina          = RazinaIskustva.Pocetnik,
            DatumKreiranja  = DateTime.Now
        };
        db.Planovi.Add(plan);
        await db.SaveChangesAsync();
        return plan;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuPlanova_Sa200()
    {
        await SeedPlan();

        var response = await _client.GetAsync("/api/planovi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<PlanDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoPocetnaRazina_VracaTocnePlanovee()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var uniq        = Guid.NewGuid().ToString("N")[..4];
        db.Planovi.Add(new Plan { Naziv = "Poc " + uniq, TrajanjeTjedana = 4, Razina = RazinaIskustva.Pocetnik, DatumKreiranja = DateTime.Now });
        db.Planovi.Add(new Plan { Naziv = "Nap " + uniq, TrajanjeTjedana = 16, Razina = RazinaIskustva.Napredni, DatumKreiranja = DateTime.Now });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/planovi?razina=Pocetnik");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<PlanDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, p => Assert.Equal("Pocetnik", p.Razina));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaPlan_KadPostoji()
    {
        var plan = await SeedPlan();

        var response = await _client.GetAsync($"/api/planovi/{plan.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PlanDTO>();
        Assert.NotNull(dto);
        Assert.Equal(plan.Id,    dto.Id);
        Assert.Equal(plan.Naziv, dto.Naziv);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/planovi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraPlan_I_Vraca201()
    {
        var payload = new PlanCreateDTO
        {
            Naziv           = "Novi Plan " + Guid.NewGuid().ToString("N")[..6],
            Opis            = "Opis novog plana",
            TrajanjeTjedana = 12,
            Razina          = RazinaIskustva.Srednji
        };

        var response = await _client.PostAsJsonAsync("/api/planovi", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PlanDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(payload.Naziv, dto.Naziv);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // TrajanjeTjedana = 0 krši [Range(1, 52)]
        var payload = new { Naziv = "Loš plan", TrajanjeTjedana = 0, Razina = 1 };

        var response = await _client.PostAsJsonAsync("/api/planovi", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraPlan_I_Vraca204()
    {
        var plan    = await SeedPlan();
        var payload = new PlanCreateDTO
        {
            Naziv           = "Ažurirani plan " + Guid.NewGuid().ToString("N")[..4],
            TrajanjeTjedana = 16,
            Razina          = RazinaIskustva.Napredni
        };

        var response = await _client.PutAsJsonAsync($"/api/planovi/{plan.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/planovi/{plan.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<PlanDTO>();
        Assert.Equal(16,          dto!.TrajanjeTjedana);
        Assert.Equal("Napredni",  dto.Razina);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new PlanCreateDTO { Naziv = "X", TrajanjeTjedana = 4, Razina = RazinaIskustva.Pocetnik };

        var response = await _client.PutAsJsonAsync("/api/planovi/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BrisePlan_I_Vraca204()
    {
        var plan = await SeedPlan();

        var deleteResponse = await _client.DeleteAsync($"/api/planovi/{plan.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/planovi/{plan.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/planovi/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
