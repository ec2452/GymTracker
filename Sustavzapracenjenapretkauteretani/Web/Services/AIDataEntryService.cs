using System.Text;
using System.Text.Json;
using Sustavzapracenjenapretkauteretani.Web.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Services;

public class AIDataEntryService(IConfiguration config, IHttpClientFactory httpClientFactory)
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string Model = "claude-haiku-4-5-20251001";

    private const string SystemPromptBase = """
        Ti si asistent za unos podataka u GymTracker aplikaciju (sustav za praćenje napretka u teretani).
        Korisnik unosi podatke na prirodnom jeziku (na hrvatskom ili engleskom).
        Tvoj zadatak je prepoznati vrstu podataka i izvući relevantna polja.

        VAŽNO: Odgovori ISKLJUČIVO s JSON objektom, bez ikakvog drugog teksta, bez markdown formatiranja, bez ```json blokova.
        Samo čisti JSON.

        Format odgovora:
        {
          "tip": "vjezba|mjerenje|trening|plan|korisnik|cilj|nepoznato",
          "podaci": { ... polja ovisno o tipu ... },
          "potvrdaPoruka": "Kratka poruka na hrvatskom što će biti kreirano (1-2 rečenice)"
        }

        Za tip "vjezba":
        podaci = {
          "naziv": string (obavezno),
          "misicnaSkupina": int 1-9 (1=Prsa 2=Leđa 3=Ramena 4=Biceps 5=Triceps 6=Noge 7=Trbuh 8=Kardio 9=CijeloTijelo),
          "razinaTezine": int 1-5 (1=najlakše, 5=najteže),
          "potrebnaOprema": bool,
          "opis": string ili null
        }

        Za tip "mjerenje":
        podaci = {
          "korisnikIme": string (puno ime, npr. "Ivan Horvat"),
          "datum": "YYYY-MM-DD" (ako nije naveden, koristi današnji datum),
          "tezina": float (u kg),
          "postotakMasti": float ili null (u %),
          "opsegStruka": float ili null (u cm)
        }

        Za tip "trening":
        podaci = {
          "korisnikIme": string (puno ime),
          "datum": "YYYY-MM-DD",
          "trajanjeMinuta": int (1-480),
          "vrsta": int 1-5 (1=Snaga 2=Kardio 3=HIIT 4=Funkcionalni 5=Mobility),
          "biljeske": string ili null,
          "potroseneKalorije": int ili null,
          "ocjena": int 1-10 ili null
        }

        Za tip "plan":
        podaci = {
          "naziv": string (obavezno),
          "trajanjeTjedana": int 1-52,
          "razina": int 1-3 (1=Početnik 2=Srednji 3=Napredni),
          "opis": string ili null
        }

        Za tip "korisnik":
        podaci = {
          "ime": string,
          "prezime": string,
          "email": string,
          "datumRodjenja": "YYYY-MM-DD",
          "visina": float (u cm, 100-250),
          "tezina": float (u kg, 30-300)
        }

        Za tip "cilj":
        podaci = {
          "korisnikIme": string (puno ime),
          "tip": int 1-5 (1=Mrsavljenje 2=DobivanjeMase 3=PovecanjeSnage 4=Izdrzljivost 5=Fleksibilnost),
          "ciljanaVrijednost": float,
          "rok": "YYYY-MM-DD"
        }

        Ako nije jasno što korisnik želi, vrati "tip": "nepoznato" i objasni u potvrdaPoruka.
        Uvijek vrati strogo validan JSON. Apsolutno NE dodavaj tekst prije ili nakon JSON-a.
        """;

    private string GetSystemPrompt() =>
        SystemPromptBase + $"\n        Danas je datum: {DateTime.Today:yyyy-MM-dd}.\n        ";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(config["AnthropicSettings:ApiKey"]);

    public async Task<AIParseResult> ParseAsync(string userInput)
    {
        var apiKey = config["AnthropicSettings:ApiKey"]
            ?? throw new InvalidOperationException("AnthropicSettings:ApiKey nije konfiguriran u appsettings.json.");

        using var http = httpClientFactory.CreateClient("anthropic");
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = JsonSerializer.Serialize(new
        {
            model = Model,
            max_tokens = 1024,
            system = GetSystemPrompt(),
            messages = new[] { new { role = "user", content = userInput } }
        });

        var response = await http.PostAsync(ApiUrl,
            new StringContent(body, Encoding.UTF8, "application/json"));

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Anthropic API greška {(int)response.StatusCode}: {responseText}");

        using var doc = JsonDocument.Parse(responseText);
        var aiText = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Ukloni moguće markdown blokove ako ih AI slučajno vrati
        aiText = aiText.Trim();
        if (aiText.StartsWith("```"))
        {
            var start = aiText.IndexOf('\n') + 1;
            var end = aiText.LastIndexOf("```");
            if (end > start) aiText = aiText[start..end].Trim();
        }

        var result = JsonSerializer.Deserialize<AIParseResult>(aiText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new AIParseResult { Tip = "nepoznato", PotvrdaPoruka = "AI nije mogao prepoznati vrstu unosa." };

        result.OriginalInput = userInput;
        return result;
    }
}
