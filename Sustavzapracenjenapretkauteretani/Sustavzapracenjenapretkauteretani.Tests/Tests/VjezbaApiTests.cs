using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class VjezbaApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public VjezbaApiTests(TeretanaWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private async Task<Vjezba> SeedVjezba()
    {
        using var scope  = _factory.Services.CreateScope();
        var db           = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var vjezba       = new Vjezba
        {
            Naziv          = "Test Vježba " + Guid.NewGuid().ToString("N")[..6],
            Opis           = "Testni opis",
            MisicnaSkupina = MisicnaSkupina.Prsa,
            RazinaTezine   = 3,
            PotrebnaOprema = false
        };
        db.Vjezbe.Add(vjezba);
        await db.SaveChangesAsync();
        return vjezba;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuVjezbi_Sa200()
    {
        await SeedVjezba();

        var response = await _client.GetAsync("/api/vjezbe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<VjezbaDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoPotrebnaOprema_VracaSamoOdgovarajuce()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        db.Vjezbe.Add(new Vjezba { Naziv = "Bez opreme " + Guid.NewGuid().ToString("N")[..4], MisicnaSkupina = MisicnaSkupina.Kardio, RazinaTezine = 1, PotrebnaOprema = false });
        db.Vjezbe.Add(new Vjezba { Naziv = "Sa opremom " + Guid.NewGuid().ToString("N")[..4], MisicnaSkupina = MisicnaSkupina.Prsa,  RazinaTezine = 2, PotrebnaOprema = true  });
        await db.SaveChangesAsync();

        var response = await _client.GetAsync("/api/vjezbe?potrebnaOprema=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<VjezbaDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, v => Assert.True(v.PotrebnaOprema));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaVjezbu_KadPostoji()
    {
        var vjezba = await SeedVjezba();

        var response = await _client.GetAsync($"/api/vjezbe/{vjezba.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<VjezbaDTO>();
        Assert.NotNull(dto);
        Assert.Equal(vjezba.Id,    dto.Id);
        Assert.Equal(vjezba.Naziv, dto.Naziv);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/vjezbe/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraVjezbu_I_Vraca201()
    {
        var payload = new VjezbaCreateDTO
        {
            Naziv          = "Nova vježba " + Guid.NewGuid().ToString("N")[..6],
            Opis           = "Opis nove vježbe",
            MisicnaSkupina = MisicnaSkupina.Noge,
            RazinaTezine   = 2,
            PotrebnaOprema = false
        };

        var response = await _client.PostAsJsonAsync("/api/vjezbe", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<VjezbaDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(payload.Naziv, dto.Naziv);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // RazinaTezine = 0 krši [Range(1, 5)]
        var payload = new { Naziv = "Vježba", MisicnaSkupina = 1, RazinaTezine = 0, PotrebnaOprema = false };

        var response = await _client.PostAsJsonAsync("/api/vjezbe", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraVjezbu_I_Vraca204()
    {
        var vjezba = await SeedVjezba();
        var payload = new VjezbaCreateDTO
        {
            Naziv          = "Izmijenjeno ime " + Guid.NewGuid().ToString("N")[..4],
            MisicnaSkupina = MisicnaSkupina.Leda,
            RazinaTezine   = 4,
            PotrebnaOprema = true
        };

        var response = await _client.PutAsJsonAsync($"/api/vjezbe/{vjezba.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Provjeri da su podaci stvarno izmijenjeni
        var getResponse = await _client.GetAsync($"/api/vjezbe/{vjezba.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<VjezbaDTO>();
        Assert.Equal(payload.Naziv, dto!.Naziv);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new VjezbaCreateDTO
        {
            Naziv          = "Nepostojeća",
            MisicnaSkupina = MisicnaSkupina.Prsa,
            RazinaTezine   = 1,
            PotrebnaOprema = false
        };

        var response = await _client.PutAsJsonAsync("/api/vjezbe/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseVjezbu_I_Vraca204()
    {
        var vjezba = await SeedVjezba();

        var deleteResponse = await _client.DeleteAsync($"/api/vjezbe/{vjezba.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Nakon brisanja (soft delete) GET mora vratiti 404
        var getResponse = await _client.GetAsync($"/api/vjezbe/{vjezba.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/vjezbe/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
