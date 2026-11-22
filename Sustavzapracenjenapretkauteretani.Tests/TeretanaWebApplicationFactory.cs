using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sustavzapracenjenapretkauteretani.DAL;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Tests;

public class TeretanaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TeretanaTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(FindMainProjectRoot());

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TeretanaDbContext"] = "InMemoryTest",
                ["Authentication:Google:ClientId"] = "test-client-id",
                ["Authentication:Google:ClientSecret"] = "test-client-secret",
                ["JwtSettings:SecretKey"] = "TestSecretKeyForJwtTokenValidationWithEnoughLength",
                ["JwtSettings:Issuer"] = "TeretanaTest",
                ["JwtSettings:Audience"] = "TeretanaUsersTest",
                ["JwtSettings:ExpirationHours"] = "24"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TeretanaDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<TeretanaDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Add test authentication
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", null!);
        });

        builder.UseEnvironment("Test");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
        SeedDomainData(db);
        return host;
    }

    private static void SeedDomainData(TeretanaDbContext db)
    {
        if (db.Vjezbe.Any()) return;

        db.Vjezbe.AddRange(
            new Vjezba { Id = 1, Naziv = "Bench Press", MisicnaSkupina = MisicnaSkupina.Prsa, RazinaTezine = 3, PotrebnaOprema = true },
            new Vjezba { Id = 2, Naziv = "Čučnjevi", MisicnaSkupina = MisicnaSkupina.Noge, RazinaTezine = 2, PotrebnaOprema = false },
            new Vjezba { Id = 3, Naziv = "Mrtvo dizanje", MisicnaSkupina = MisicnaSkupina.Leda, RazinaTezine = 4, PotrebnaOprema = true },
            new Vjezba { Id = 4, Naziv = "Biceps curl", MisicnaSkupina = MisicnaSkupina.Biceps, RazinaTezine = 2, PotrebnaOprema = true },
            new Vjezba { Id = 5, Naziv = "Plank", MisicnaSkupina = MisicnaSkupina.Trbuh, RazinaTezine = 1, PotrebnaOprema = false },
            new Vjezba { Id = 6, Naziv = "Trčanje", MisicnaSkupina = MisicnaSkupina.Kardio, RazinaTezine = 1, PotrebnaOprema = false },
            new Vjezba { Id = 7, Naziv = "Triceps dips", MisicnaSkupina = MisicnaSkupina.Triceps, RazinaTezine = 2, PotrebnaOprema = true },
            new Vjezba { Id = 8, Naziv = "Sklekovi", MisicnaSkupina = MisicnaSkupina.Prsa, RazinaTezine = 1, PotrebnaOprema = false }
        );

        db.Planovi.AddRange(
            new Plan { Id = 1, Naziv = "Početni plan", Opis = "Za početnike.", TrajanjeTjedana = 8, Razina = RazinaIskustva.Pocetnik, DatumKreiranja = new DateTime(2025, 1, 1) },
            new Plan { Id = 2, Naziv = "Snaga i masa", Opis = "Povećanje mišićne mase.", TrajanjeTjedana = 12, Razina = RazinaIskustva.Srednji, DatumKreiranja = new DateTime(2025, 1, 1) },
            new Plan { Id = 3, Naziv = "Mršavljenje", Opis = "Gubitak mase.", TrajanjeTjedana = 10, Razina = RazinaIskustva.Srednji, DatumKreiranja = new DateTime(2025, 1, 1) },
            new Plan { Id = 4, Naziv = "Napredni plan", Opis = "Za iskusne korisnike.", TrajanjeTjedana = 16, Razina = RazinaIskustva.Napredni, DatumKreiranja = new DateTime(2025, 1, 1) }
        );

        db.Korisnici.AddRange(
            new Korisnik { Id = 1, Ime = "Marko", Prezime = "Horvat", Kilaza = 80, Visina = 180, Pol = Pol.Musko },
            new Korisnik { Id = 2, Ime = "Ana", Prezime = "Horvat", Kilaza = 65, Visina = 170, Pol = Pol.Zensko },
            new Korisnik { Id = 3, Ime = "Ivan", Prezime = "Horvat", Kilaza = 90, Visina = 190, Pol = Pol.Musko }
        );

        db.Ciljevi.AddRange(
            new Cilj { Id = 1, KorisnikId = 1, Tip = TipCilja.Mrsavljenje, TargetKilaza = 75, Rok = DateTime.UtcNow.AddMonths(3), DatumKreiranja = DateTime.UtcNow },
            new Cilj { Id = 2, KorisnikId = 2, Tip = TipCilja.Prijedena, TargetKilaza = 60, Rok = DateTime.UtcNow.AddMonths(6), DatumKreiranja = DateTime.UtcNow }
        );

        db.Mjerenja.AddRange(
            new Mjerenje { Id = 1, KorisnikId = 1, Datum = DateTime.UtcNow, Kilaza = 80, Napomena = "Početno mjerenje" },
            new Mjerenje { Id = 2, KorisnikId = 2, Datum = DateTime.UtcNow, Kilaza = 65, Napomena = "Početno mjerenje" }
        );

        db.Treninzi.AddRange(
            new Trening { Id = 1, KorisnikId = 1, Datum = DateTime.UtcNow, Napomena = "Test trening" },
            new Trening { Id = 2, KorisnikId = 2, Datum = DateTime.UtcNow, Napomena = "Test trening 2" }
        );

        db.SaveChanges();
    }

    private static string FindMainProjectRoot()
    {
        var current = AppDomain.CurrentDomain.BaseDirectory;
        while (current != null)
        {
            if (File.Exists(Path.Combine(current, "Program.cs")))
                return current;
            current = Directory.GetParent(current)?.FullName;
        }
        return AppDomain.CurrentDomain.BaseDirectory;
    }
}
