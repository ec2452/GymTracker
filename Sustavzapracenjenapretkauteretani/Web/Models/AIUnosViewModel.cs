using System.Text.Json.Serialization;

namespace Sustavzapracenjenapretkauteretani.Web.Models;

public class AIUnosViewModel
{
    public string? KorisnickiUnos { get; set; }
    public string? UspjesnaPoruka { get; set; }
    public string? GreskaPoruka { get; set; }
    public bool ApiKljucNijePostavljen { get; set; }
}

public class AIParseResult
{
    [JsonPropertyName("tip")]
    public string Tip { get; set; } = "";

    [JsonPropertyName("podaci")]
    public System.Text.Json.JsonElement Podaci { get; set; }

    [JsonPropertyName("potvrdaPoruka")]
    public string PotvrdaPoruka { get; set; } = "";

    public string? OriginalInput { get; set; }
}

// ── DTOs za svaki tip entiteta koje AI vraca ──────────────────────────────────

public class VjezbaAIData
{
    [JsonPropertyName("naziv")]
    public string Naziv { get; set; } = "";

    [JsonPropertyName("misicnaSkupina")]
    public int MisicnaSkupina { get; set; } = 1;

    [JsonPropertyName("razinaTezine")]
    public int RazinaTezine { get; set; } = 1;

    [JsonPropertyName("potrebnaOprema")]
    public bool PotrebnaOprema { get; set; }

    [JsonPropertyName("opis")]
    public string? Opis { get; set; }
}

public class MjerenjeAIData
{
    [JsonPropertyName("korisnikIme")]
    public string KorisnikIme { get; set; } = "";

    [JsonPropertyName("datum")]
    public string Datum { get; set; } = "";

    [JsonPropertyName("tezina")]
    public double Tezina { get; set; }

    [JsonPropertyName("postotakMasti")]
    public double? PostotakMasti { get; set; }

    [JsonPropertyName("opsegStruka")]
    public double? OpsegStruka { get; set; }
}

public class TreningAIData
{
    [JsonPropertyName("korisnikIme")]
    public string KorisnikIme { get; set; } = "";

    [JsonPropertyName("datum")]
    public string Datum { get; set; } = "";

    [JsonPropertyName("trajanjeMinuta")]
    public int TrajanjeMinuta { get; set; }

    [JsonPropertyName("vrsta")]
    public int Vrsta { get; set; } = 1;

    [JsonPropertyName("biljeske")]
    public string? Biljeske { get; set; }

    [JsonPropertyName("potroseneKalorije")]
    public int? PotroseneKalorije { get; set; }

    [JsonPropertyName("ocjena")]
    public int? Ocjena { get; set; }
}

public class PlanAIData
{
    [JsonPropertyName("naziv")]
    public string Naziv { get; set; } = "";

    [JsonPropertyName("trajanjeTjedana")]
    public int TrajanjeTjedana { get; set; } = 4;

    [JsonPropertyName("razina")]
    public int Razina { get; set; } = 1;

    [JsonPropertyName("opis")]
    public string? Opis { get; set; }
}

public class KorisnikAIData
{
    [JsonPropertyName("ime")]
    public string Ime { get; set; } = "";

    [JsonPropertyName("prezime")]
    public string Prezime { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("datumRodjenja")]
    public string DatumRodjenja { get; set; } = "";

    [JsonPropertyName("visina")]
    public double Visina { get; set; }

    [JsonPropertyName("tezina")]
    public double Tezina { get; set; }
}

public class CiljAIData
{
    [JsonPropertyName("korisnikIme")]
    public string KorisnikIme { get; set; } = "";

    [JsonPropertyName("tip")]
    public int Tip { get; set; } = 1;

    [JsonPropertyName("ciljanaVrijednost")]
    public double CiljanaVrijednost { get; set; }

    [JsonPropertyName("rok")]
    public string Rok { get; set; } = "";
}
