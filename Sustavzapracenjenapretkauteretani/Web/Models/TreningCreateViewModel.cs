using System;
using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class TreningCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Korisnik je obavezan.")]
        public int? KorisnikId { get; set; }

        public string KorisnikNaziv { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum treninga je obavezan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum i vrijeme")]
        public DateTime DatumVrijeme { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Trajanje je obavezno.")]
        [Range(1, 480, ErrorMessage = "Trajanje mora biti između 1 i 480 minuta.")]
        [Display(Name = "Trajanje (min)")]
        public int TrajanjeMinuta { get; set; }

        [Required(ErrorMessage = "Vrsta treninga je obavezna.")]
        [Display(Name = "Vrsta treninga")]
        public VrstaTreninga Vrsta { get; set; }

        [StringLength(500, MinimumLength = 3, ErrorMessage = "Bilješke moraju biti između 3 i 500 znakova.")]
        [Display(Name = "Bilješke")]
        public string? Biljeske { get; set; }

        [Range(0, 5000, ErrorMessage = "Potrošene kalorije moraju biti između 0 i 5000.")]
        [Display(Name = "Potrošene kalorije")]
        public int? PotroseneKalorije { get; set; }

        [Range(1, 10, ErrorMessage = "Ocjena mora biti između 1 i 10.")]
        [Display(Name = "Ocjena (1-10)")]
        public int? Ocjena { get; set; }

        public List<TreningKorisnikOption> AvailableKorisnici { get; set; } = new();
    }

    public class TreningKorisnikOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
