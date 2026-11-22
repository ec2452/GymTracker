using System;
using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class PlanCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Naziv plana je obavezan.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Naziv mora biti između 3 i 100 znakova.")]
        [Display(Name = "Naziv plana")]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(500, MinimumLength = 3, ErrorMessage = "Opis mora biti između 3 i 500 znakova.")]
        [Display(Name = "Opis")]
        public string? Opis { get; set; }

        [Required(ErrorMessage = "Trajanje je obavezno.")]
        [Range(1, 52, ErrorMessage = "Trajanje mora biti između 1 i 52 tjedna.")]
        [Display(Name = "Trajanje (tjedni)")]
        public int TrajanjeTjedana { get; set; }

        [Required(ErrorMessage = "Razina iskustva je obavezna.")]
        [Display(Name = "Razina iskustva")]
        public RazinaIskustva Razina { get; set; }
    }
}
