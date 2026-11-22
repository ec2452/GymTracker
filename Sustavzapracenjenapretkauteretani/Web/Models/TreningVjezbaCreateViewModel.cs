using System;
using System.ComponentModel.DataAnnotations;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class TreningVjezbaCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Trening je obavezan.")]
        [Display(Name = "Trening")]
        public int? TreningId { get; set; }

        [Required(ErrorMessage = "Vježba je obavezna.")]
        [Display(Name = "Vježba")]
        public int? VjezbaId { get; set; }

        [Required(ErrorMessage = "Broj serija je obavezan.")]
        [Range(1, 20, ErrorMessage = "Broj serija mora biti između 1 i 20.")]
        [Display(Name = "Broj serija")]
        public int BrojSerija { get; set; }

        [Required(ErrorMessage = "Broj ponavljanja je obavezan.")]
        [Range(1, 100, ErrorMessage = "Broj ponavljanja mora biti između 1 i 100.")]
        [Display(Name = "Ponavljanja po seriji")]
        public int BrojPonavljanja { get; set; }

        [Range(0, 500, ErrorMessage = "Težina mora biti između 0 i 500 kg.")]
        [Display(Name = "Težina (kg)")]
        public double? TezinaKg { get; set; }

        [Required(ErrorMessage = "Odmor je obavezan.")]
        [Range(0, 600, ErrorMessage = "Odmor mora biti između 0 i 600 sekundi.")]
        [Display(Name = "Odmor između serija (sek)")]
        public int OdmorSekundi { get; set; }

        [Required(ErrorMessage = "Redoslijed je obavezan.")]
        [Range(1, 100, ErrorMessage = "Redoslijed mora biti između 1 i 100.")]
        [Display(Name = "Redoslijed u treningu")]
        public int Redoslijed { get; set; }

        public string TreningDisplayText { get; set; } = string.Empty;

        public string VjezbaDisplayText { get; set; } = string.Empty;
    }

}
