using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class KorisnikCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora biti između 2 i 50 znakova.")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora biti između 2 i 50 znakova.")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravan email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rođenja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum rođenja")]
        public DateTime DatumRodjenja { get; set; }

        [Required(ErrorMessage = "Visina je obavezna.")]
        [Range(100, 250, ErrorMessage = "Visina mora biti između 100 i 250 cm.")]
        [Display(Name = "Visina (cm)")]
        public double Visina { get; set; }

        [Required(ErrorMessage = "Trenutna težina je obavezna.")]
        [Range(30, 300, ErrorMessage = "Težina mora biti između 30 i 300 kg.")]
        [Display(Name = "Trenutna težina (kg)")]
        public double Tezina { get; set; }

        [Display(Name = "Plan treninga")]
        public int? PlanId { get; set; }

        [Required(ErrorMessage = "Datum mjerenja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum početnog mjerenja")]
        public DateTime DatumMjerenja { get; set; }

        [Required(ErrorMessage = "Težina na mjerenju je obavezna.")]
        [Range(30, 300, ErrorMessage = "Težina mora biti između 30 i 300 kg.")]
        [Display(Name = "Težina na mjerenju (kg)")]
        public double MjerenjeTezina { get; set; }

        [Range(3, 60, ErrorMessage = "Postotak masti mora biti između 3 i 60.")]
        [Display(Name = "Postotak masti (%)")]
        public double? PostotakMasti { get; set; }

        [Range(40, 200, ErrorMessage = "Opseg struka mora biti između 40 i 200 cm.")]
        [Display(Name = "Opseg struka (cm)")]
        public double? OpsegStruka { get; set; }

        [Required(ErrorMessage = "Tip cilja je obavezan.")]
        [Display(Name = "Tip cilja")]
        public TipCilja TipCilja { get; set; }

        [Required(ErrorMessage = "Ciljana vrijednost je obavezna.")]
        [Range(0.1, 10000, ErrorMessage = "Ciljana vrijednost mora biti veća od 0.")]
        [Display(Name = "Ciljana vrijednost")]
        public double CiljanaVrijednost { get; set; }

        [Required(ErrorMessage = "Rok cilja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Rok cilja")]
        public DateTime RokCilja { get; set; }

        public List<PlanOption> AvailablePlanovi { get; set; } = new();
    }

    public class PlanOption
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }
}
