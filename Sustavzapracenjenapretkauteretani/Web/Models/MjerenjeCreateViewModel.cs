using System;
using System.ComponentModel.DataAnnotations;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class MjerenjeCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Korisnik je obavezan.")]
        public int? KorisnikId { get; set; }

        [Required(ErrorMessage = "Datum mjerenja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum mjerenja")]
        public DateTime DatumMjerenja { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Težina je obavezna.")]
        [Range(30, 300, ErrorMessage = "Težina mora biti između 30 i 300 kg.")]
        [Display(Name = "Težina (kg)")]
        public double Tezina { get; set; }

        [Range(3, 60, ErrorMessage = "Postotak masti mora biti između 3 i 60%.")]
        [Display(Name = "Postotak masti (%)")]
        public double? PostotakMasti { get; set; }

        [Range(40, 200, ErrorMessage = "Opseg struka mora biti između 40 i 200 cm.")]
        [Display(Name = "Opseg struka (cm)")]
        public double? OpsegStruka { get; set; }

        public string KorisnikNaziv { get; set; } = string.Empty;

        public List<MjerenjeKorisnikOption> AvailableKorisnici { get; set; } = new();
    }

    public class MjerenjeKorisnikOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
