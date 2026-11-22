using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sustavzapracenjenapretkauteretani.DAL;

namespace Sustavzapracenjenapretkauteretani.Tests.Helpers;

public class TeretanaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TeretanaTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Lažne konfiguracije — sprječavaju bacanje iznimki pri pokretanju
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"]              = "test-secret-key-minimum-32-chars-ok!",
                ["JwtSettings:Issuer"]                 = "test-issuer",
                ["JwtSettings:Audience"]               = "test-audience",
                ["Authentication:Google:ClientId"]     = "fake-google-client-id",
                ["Authentication:Google:ClientSecret"] = "fake-google-client-secret",
                ["ConnectionStrings:TeretanaDbContext"] = "FakeTestConnection"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Ukloni SQL Server DbContext, dodaj InMemory
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TeretanaDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<TeretanaDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Nadjači autentikaciju testnim handlerom koji uvijek vraća Admin korisnika.
            // TestAuthHandler se registrira i za zadanu "Test" shemu i za "Bearer" (JWT) shemu
            // jer API controlleri koriste [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)].
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigureAll<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = "Test";
                opts.DefaultChallengeScheme    = "Test";
                opts.DefaultForbidScheme       = "Test";

                // Preusmjeri JWT/Bearer shemu na TestAuthHandler modificiranjem postojećeg buildera
                // u-place (SchemeMap i interni Schemes popis dijele isti objekt pa HandlerType mora
                // biti promijenjen na samom builderu, ne zamjenom reference u SchemeMap).
                if (opts.SchemeMap.TryGetValue(JwtBearerDefaults.AuthenticationScheme, out var jwtBuilder))
                    jwtBuilder.HandlerType = typeof(TestAuthHandler);
            });
        });
    }

    // Pomoćna metoda — vraća DbContext za seedanje podataka u testovima
    public TeretanaDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TeretanaDbContext>();
    }
}
