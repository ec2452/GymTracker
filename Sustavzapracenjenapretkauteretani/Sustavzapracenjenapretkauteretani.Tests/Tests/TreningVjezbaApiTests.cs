using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Tests.Helpers;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests.Tests;

public class TreningVjezbaApiTests : IClassFixture<TeretanaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TeretanaWebApplicationFactory _factory;

    public TreningVjezbaApiTests(TeretanaWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private async Task<(Trening trening, Vjezba vjezba)> SeedTreningIVjezba()
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var uniq        = Guid.NewGuid().ToString("N")[..6];

        var korisnik = new Korisnik
        {
            Ime = "TV", Prezime = "K", Email = uniq + "@tv.com",
            DatumRodjenja = new DateTime(1991, 4, 4), DatumRegistracije = DateTime.Now,
            Visina = 175, Tezina = 78
        };
        db.Korisnici.Add(korisnik);
        await db.SaveChangesAsync();

        var vjezba = new Vjezba
        {
            Naziv = "TV Vježba " + uniq, MisicnaSkupina = MisicnaSkupina.Biceps, RazinaTezine = 2, PotrebnaOprema = true
        };
        db.Vjezbe.Add(vjezba);
        await db.SaveChangesAsync();

        var trening = new Trening
        {
            DatumVrijeme = DateTime.Now, TrajanjeMinuta = 60, Vrsta = VrstaTreninga.Snaga, KorisnikId = korisnik.Id
        };
        db.Treninzi.Add(trening);
        await db.SaveChangesAsync();

        return (trening, vjezba);
    }

    private async Task<TreningVjezba> SeedTreningVjezba(int treningId, int vjezbaId)
    {
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        var tv = new TreningVjezba
        {
            TreningId      = treningId,
            VjezbaId       = vjezbaId,
            BrojSerija     = 3,
            BrojPonavljanja = 10,
            TezinaKg       = 20,
            OdmorSekundi   = 90,
            Redoslijed     = 1
        };
        db.TreningVjezbe.Add(tv);
        await db.SaveChangesAsync();
        return tv;
    }

    // ─── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_VracaListuTreningVjezbi_Sa200()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        await SeedTreningVjezba(trening.Id, vjezba.Id);

        var response = await _client.GetAsync("/api/trening-vjezbe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<TreningVjezbaDTO>>();
        Assert.NotNull(lista);
        Assert.True(lista.Count > 0);
    }

    [Fact]
    public async Task GetAll_FiltriranjePoBrojuTreninga_VracaTocneVjezbe()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        await SeedTreningVjezba(trening.Id, vjezba.Id);

        var response = await _client.GetAsync($"/api/trening-vjezbe?treningId={trening.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await response.Content.ReadFromJsonAsync<List<TreningVjezbaDTO>>();
        Assert.NotNull(lista);
        Assert.All(lista, tv => Assert.Equal(trening.Id, tv.TreningId));
    }

    // ─── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_VracaTreningVjezbu_KadPostoji()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        var tv                = await SeedTreningVjezba(trening.Id, vjezba.Id);

        var response = await _client.GetAsync($"/api/trening-vjezbe/{tv.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TreningVjezbaDTO>();
        Assert.NotNull(dto);
        Assert.Equal(tv.Id,       dto.Id);
        Assert.Equal(trening.Id,  dto.TreningId);
    }

    [Fact]
    public async Task GetById_Vraca404_KadNePostoji()
    {
        var response = await _client.GetAsync("/api/trening-vjezbe/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── POST ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Post_KreiraTreningVjezbu_I_Vraca201()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        var payload = new TreningVjezbaCreateDTO
        {
            TreningId      = trening.Id,
            VjezbaId       = vjezba.Id,
            BrojSerija     = 4,
            BrojPonavljanja = 8,
            TezinaKg       = 30,
            OdmorSekundi   = 120,
            Redoslijed     = 2
        };

        var response = await _client.PostAsJsonAsync("/api/trening-vjezbe", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<TreningVjezbaDTO>();
        Assert.NotNull(dto);
        Assert.True(dto.Id > 0);
        Assert.Equal(trening.Id, dto.TreningId);
    }

    [Fact]
    public async Task Post_Vraca400_KadValidacijaNijeIspravna()
    {
        // BrojSerija = 0 krši [Range(1, 20)]
        var payload = new { TreningId = 1, VjezbaId = 1, BrojSerija = 0, BrojPonavljanja = 10, OdmorSekundi = 60, Redoslijed = 1 };

        var response = await _client.PostAsJsonAsync("/api/trening-vjezbe", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ─── PUT ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Put_AzuriraTreningVjezbu_I_Vraca204()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        var tv                = await SeedTreningVjezba(trening.Id, vjezba.Id);
        var payload = new TreningVjezbaCreateDTO
        {
            TreningId      = trening.Id,
            VjezbaId       = vjezba.Id,
            BrojSerija     = 5,
            BrojPonavljanja = 12,
            TezinaKg       = 25,
            OdmorSekundi   = 60,
            Redoslijed     = 1
        };

        var response = await _client.PutAsJsonAsync($"/api/trening-vjezbe/{tv.Id}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/trening-vjezbe/{tv.Id}");
        var dto         = await getResponse.Content.ReadFromJsonAsync<TreningVjezbaDTO>();
        Assert.Equal(5,  dto!.BrojSerija);
        Assert.Equal(12, dto.BrojPonavljanja);
    }

    [Fact]
    public async Task Put_Vraca404_KadNePostoji()
    {
        var payload = new TreningVjezbaCreateDTO
        {
            TreningId = 1, VjezbaId = 1, BrojSerija = 3, BrojPonavljanja = 10, OdmorSekundi = 60, Redoslijed = 1
        };

        var response = await _client.PutAsJsonAsync("/api/trening-vjezbe/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_BriseTreningVjezbu_I_Vraca204()
    {
        var (trening, vjezba) = await SeedTreningIVjezba();
        var tv                = await SeedTreningVjezba(trening.Id, vjezba.Id);

        var deleteResponse = await _client.DeleteAsync($"/api/trening-vjezbe/{tv.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/trening-vjezbe/{tv.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_Vraca404_KadNePostoji()
    {
        var response = await _client.DeleteAsync("/api/trening-vjezbe/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
