using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;
using Sustavzapracenjenapretkauteretani.Web.Filters;
using Sustavzapracenjenapretkauteretani.Web.Mcp;
using Sustavzapracenjenapretkauteretani.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Teretana.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

// ── SERILOG – inicijalizacija prije izgradnje aplikacije ──────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/app/teretana-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "Logs/json/teretana-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ==== REGISTRACIJA ENTITY FRAMEWORK ====
builder.Services.AddDbContext<TeretanaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TeretanaDbContext")));

// ==== IDENTITY ====
builder.Services
    .AddDefaultIdentity<AppUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<TeretanaDbContext>();

// ==== JWT + GOOGLE OAUTH ====
// Zadana shema ostaje Identity cookie (postavljeno u AddDefaultIdentity gore).
// API controlleri eksplicitno traže JWT shemu via [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)].
var authBuilder = builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ??
            throw new InvalidOperationException("JWT SecretKey nije konfiguriran"));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var googleClientId     = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId     = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// ==== REGISTRACIJA REPOSITORY KLASA ====
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<KorisnikRepository>();
builder.Services.AddScoped<TreningRepository>();
builder.Services.AddScoped<VjezbaRepository>();
builder.Services.AddScoped<PlanRepository>();
builder.Services.AddScoped<CiljRepository>();
builder.Services.AddScoped<MjerenjeRepository>();
builder.Services.AddScoped<TreningVjezbaRepository>();

// Add services to the container.
builder.Services.AddScoped<NavBadgeFilter>();
builder.Services.AddScoped<AuditLogFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<NavBadgeFilter>();
    options.Filters.AddService<AuditLogFilter>();
});
builder.Services.AddRazorPages();

// ==== AI DATA ENTRY SERVICE ====
builder.Services.AddHttpClient("anthropic");
builder.Services.AddScoped<AIDataEntryService>();

// ==== MCP SERVER – agentic IDE pristup ====
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<TeretanaMcpTools>();

// ==== KONFIGURACIJA VIEW LOKACIJA ====
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Web/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Web/Views/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serilog middleware — logira svaki HTTP zahtjev (metoda, putanja, status, trajanje)
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0}ms)";
    opts.GetLevel = (ctx, elapsed, ex) =>
        ex is not null || ctx.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : ctx.Response.StatusCode >= 400
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
});

app.UseRouting();

// ==== LOKALIZACIJA – preuzima jezik iz Accept-Language headera preglednika ====
var supportedCultures = new[]
{
    new CultureInfo("hr"),
    new CultureInfo("en-US")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures     = supportedCultures,
    SupportedUICultures   = supportedCultures
});

// UseAuthentication mora biti PRIJE UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ==== MCP endpoint — dostupan na http://localhost:5073/mcp ====
app.MapMcp("/mcp");

// ==== SEED ULOGA I ADMIN KORISNIKA ====
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager, userManager);
}

app.Run();

public partial class Program { }
