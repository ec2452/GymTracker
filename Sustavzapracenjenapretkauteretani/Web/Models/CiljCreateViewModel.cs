using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class CiljCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Tip cilja je obavezan.")]
        [Display(Name = "Tip cilja")]
        public TipCilja Tip { get; set; }

        [Required(ErrorMessage = "Ciljana vrijednost je obavezna.")]
        [Range(0.1, 10000, ErrorMessage = "Vrijednost mora biti između 0.1 i 10000.")]
        [Display(Name = "Ciljana vrijednost")]
        public double CiljanaVrijednost { get; set; }

        [Required(ErrorMessage = "Rok za ostvarenje je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Rok za ostvarenje")]
        public DateTime Rok { get; set; }

        [Display(Name = "Cilj je postignut")]
        public bool Postignut { get; set; }

        [Required(ErrorMessage = "Korisnik je obavezan.")]
        [Display(Name = "Korisnik")]
        public int? KorisnikId { get; set; }

        // Za autocomplete inicijalne vrijednosti (Edit akcija)
        public string KorisnikNaziv { get; set; } = string.Empty;

        public List<KorisnikOption> AvailableKorisnici { get; set; } = new();
    }

    public class KorisnikOption
    {
        public int Id { get; set; }
        public string ImeIPrezime { get; set; } = string.Empty;
    }
}
