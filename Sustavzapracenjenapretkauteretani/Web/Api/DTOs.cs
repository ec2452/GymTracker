using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Api
{
    // ==================== AUTH / USER INFO ====================

    public class UserInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    // ==================== VJEZBA ====================

    public class VjezbaDTO
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public string MisicnaSkupina { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int RazinaTezine { get; set; }
        public bool PotrebnaOprema { get; set; }
    }

    public class VjezbaCreateDTO
    {
        [Required]
        [StringLength(100)]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Opis { get; set; }

        [Range(1, 9)]
        public MisicnaSkupina MisicnaSkupina { get; set; }

        [Url]
        public string? VideoUrl { get; set; }

        [Range(1, 5)]
        public int RazinaTezine { get; set; }

        public bool PotrebnaOprema { get; set; }
    }

    // ==================== KORISNIK ====================

    public class KorisnikDTO
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DatumRodjenja { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public double Visina { get; set; }
        public double Tezina { get; set; }
        public int? PlanId { get; set; }
    }

    public class KorisnikCreateDTO
    {
        [Required]
        [StringLength(50)]
        public string Ime { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Prezime { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DatumRodjenja { get; set; }

        [Range(100, 250)]
        public double Visina { get; set; }

        [Range(30, 300)]
        public double Tezina { get; set; }

        public int? PlanId { get; set; }
    }

    // ==================== TRENING ====================

    public class TreningDTO
    {
        public int Id { get; set; }
        public DateTime DatumVrijeme { get; set; }
        public int TrajanjeMinuta { get; set; }
        public string Vrsta { get; set; } = string.Empty;
        public string? Biljeske { get; set; }
        public int? PotroseneKalorije { get; set; }
        public int? Ocjena { get; set; }
        public int KorisnikId { get; set; }
    }

    public class TreningCreateDTO
    {
        [Required]
        public DateTime DatumVrijeme { get; set; }

        [Range(1, 480)]
        public int TrajanjeMinuta { get; set; }

        [Range(1, 5)]
        public VrstaTreninga Vrsta { get; set; }

        [StringLength(500)]
        public string? Biljeske { get; set; }

        [Range(0, 5000)]
        public int? PotroseneKalorije { get; set; }

        [Range(1, 10)]
        public int? Ocjena { get; set; }

        [Required]
        public int KorisnikId { get; set; }
    }

    // ==================== PLAN ====================

    public class PlanDTO
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public int TrajanjeTjedana { get; set; }
        public string Razina { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; }
        public List<VjezbaDTO> Vjezbe { get; set; } = new();
        public int BrojKorisnika { get; set; }
    }

    public class PlanCreateDTO
    {
        [Required]
        [StringLength(100)]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Opis { get; set; }

        [Range(1, 52)]
        public int TrajanjeTjedana { get; set; }

        [Range(1, 3)]
        public RazinaIskustva Razina { get; set; }
    }

    // ==================== MJERENJE ====================

    public class MjerenjeDTO
    {
        public int Id { get; set; }
        public DateTime DatumMjerenja { get; set; }
        public double Tezina { get; set; }
        public double? PostotakMasti { get; set; }
        public double? OpsegStruka { get; set; }
        public int KorisnikId { get; set; }
    }

    public class MjerenjeCreateDTO
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime DatumMjerenja { get; set; }

        [Range(30, 300)]
        public double Tezina { get; set; }

        [Range(3, 60)]
        public double? PostotakMasti { get; set; }

        [Range(40, 200)]
        public double? OpsegStruka { get; set; }

        [Required]
        public int KorisnikId { get; set; }
    }

    // ==================== CILJ ====================

    public class CiljDTO
    {
        public int Id { get; set; }
        public string Tip { get; set; } = string.Empty;
        public double CiljanaVrijednost { get; set; }
        public DateTime Rok { get; set; }
        public bool Postignut { get; set; }
        public int KorisnikId { get; set; }
    }

    public class CiljCreateDTO
    {
        [Range(1, 5)]
        public TipCilja Tip { get; set; }

        [Range(0.1, 10000)]
        public double CiljanaVrijednost { get; set; }

        [DataType(DataType.Date)]
        public DateTime Rok { get; set; }

        public bool Postignut { get; set; }

        [Required]
        public int KorisnikId { get; set; }
    }

    // ==================== TRENING VJEZBA ====================

    public class TreningVjezbaDTO
    {
        public int Id { get; set; }
        public int TreningId { get; set; }
        public VjezbaDTO? Vjezba { get; set; }
        public int BrojSerija { get; set; }
        public int BrojPonavljanja { get; set; }
        public double? TezinaKg { get; set; }
        public int OdmorSekundi { get; set; }
        public int Redoslijed { get; set; }
    }

    public class TreningVjezbaCreateDTO
    {
        [Required]
        public int TreningId { get; set; }

        [Required]
        public int VjezbaId { get; set; }

        [Range(1, 20)]
        public int BrojSerija { get; set; }

        [Range(1, 100)]
        public int BrojPonavljanja { get; set; }

        [Range(0, 500)]
        public double? TezinaKg { get; set; }

        [Range(0, 600)]
        public int OdmorSekundi { get; set; }

        [Range(1, 100)]
        public int Redoslijed { get; set; }
    }
}
