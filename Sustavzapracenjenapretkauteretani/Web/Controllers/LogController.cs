using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("admin/logovi")]
[Authorize(Roles = "Admin")]
public class LogController : Controller
{
    private static readonly string JsonLogsDir =
        Path.Combine(Directory.GetCurrentDirectory(), "Logs", "json");

    // ─── VIEW ──────────────────────────────────────────────────────────────────

    [HttpGet("", Name = "LogoviIndex")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Pregled logova";
        return View();
    }

    // ─── JSON API ──────────────────────────────────────────────────────────────
    // GET /admin/logovi/podaci?razina=Error&pretraga=Audit&stranica=1&velicina=50

    [HttpGet("podaci")]
    public IActionResult GetPodaci(
        [FromQuery] string?  razina   = null,
        [FromQuery] string?  pretraga = null,
        [FromQuery] int      stranica = 1,
        [FromQuery] int      velicina = 100)
    {
        var entries = CitajLogove(razina, pretraga);
        var ukupno  = entries.Count;
        var page    = entries
            .Skip((stranica - 1) * velicina)
            .Take(velicina)
            .ToList();

        return Json(new
        {
            ukupno,
            stranica,
            velicina,
            stranice = (int)Math.Ceiling(ukupno / (double)velicina),
            zapisi   = page
        });
    }

    // ─── STATISTIKE ────────────────────────────────────────────────────────────

    [HttpGet("statistike")]
    public IActionResult GetStatistike()
    {
        var entries = CitajLogove();
        return Json(new
        {
            ukupno      = entries.Count,
            information = entries.Count(e => e.Razina == "Information"),
            warning     = entries.Count(e => e.Razina == "Warning"),
            error       = entries.Count(e => e.Razina is "Error" or "Fatal"),
            prvizapis   = entries.LastOrDefault()?.Timestamp,
            zadnjiZapis = entries.FirstOrDefault()?.Timestamp
        });
    }

    // ─── POMOĆNE METODE ────────────────────────────────────────────────────────

    private static List<LogZapis> CitajLogove(string? razina = null, string? pretraga = null)
    {
        var rezultati = new List<LogZapis>();
        if (!Directory.Exists(JsonLogsDir)) return rezultati;

        // Sve JSON log datoteke, sortirane od najnovije
        var datoteke = Directory.GetFiles(JsonLogsDir, "*.json")
                                .OrderByDescending(f => f)
                                .Take(3); // max zadnja 3 dana

        foreach (var datoteka in datoteke)
        {
            try
            {
                using var fs     = new FileStream(datoteka, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, System.Text.Encoding.UTF8);
                var sve = new List<string>();
                while (!reader.EndOfStream)
                {
                    var linija = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(linija)) sve.Add(linija);
                }
                foreach (var linija in sve.AsEnumerable().Reverse().Take(2000))
                {
                    var zapis = ParsiraClef(linija);
                    if (zapis is null) continue;
                    rezultati.Add(zapis);
                }
            }
            catch { /* preskoči nedostupnu datoteku */ }
        }

        // Primijeni filtre
        var upit = rezultati.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(razina) && razina != "Sve")
            upit = upit.Where(e => string.Equals(e.Razina, razina, StringComparison.OrdinalIgnoreCase)
                                || (razina == "Error" && e.Razina == "Fatal"));

        if (!string.IsNullOrWhiteSpace(pretraga))
            upit = upit.Where(e => e.Poruka.Contains(pretraga, StringComparison.OrdinalIgnoreCase)
                                || (e.Iznimka != null && e.Iznimka.Contains(pretraga, StringComparison.OrdinalIgnoreCase)));

        return upit.OrderByDescending(e => e.Timestamp).ToList();
    }

    private static LogZapis? ParsiraClef(string linija)
    {
        try
        {
            using var doc  = JsonDocument.Parse(linija);
            var root = doc.RootElement;

            // @t — timestamp
            var ts = root.TryGetProperty("@t", out var t)
                ? DateTime.Parse(t.GetString()!, null, System.Globalization.DateTimeStyles.RoundtripKind)
                : DateTime.UtcNow;

            // @l — level (ako nedostaje = Information)
            var razina = root.TryGetProperty("@l", out var l) ? l.GetString() ?? "Information" : "Information";

            // @m — rendered message; fallback @mt template
            string poruka;
            if (root.TryGetProperty("@m", out var m) && !string.IsNullOrWhiteSpace(m.GetString()))
                poruka = m.GetString()!;
            else if (root.TryGetProperty("@mt", out var mt))
                poruka = mt.GetString() ?? "";
            else
                poruka = "";

            // @x — exception
            var iznimka = root.TryGetProperty("@x", out var x) ? x.GetString() : null;

            // SourceContext
            var izvor = root.TryGetProperty("SourceContext", out var sc) ? sc.GetString() : null;
            if (izvor is not null)
            {
                // Skrati na samo ime klase
                var dot = izvor.LastIndexOf('.');
                if (dot >= 0) izvor = izvor[(dot + 1)..];
            }

            return new LogZapis
            {
                Timestamp = ts.ToLocalTime(),
                Razina    = razina,
                Poruka    = poruka,
                Iznimka   = iznimka,
                Izvor     = izvor
            };
        }
        catch { return null; }
    }

    public class LogZapis
    {
        public DateTime Timestamp { get; set; }
        public string   Razina    { get; set; } = "Information";
        public string   Poruka    { get; set; } = "";
        public string?  Iznimka   { get; set; }
        public string?  Izvor     { get; set; }
    }
}
