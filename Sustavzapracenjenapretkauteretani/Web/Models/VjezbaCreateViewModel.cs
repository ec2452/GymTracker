using System.ComponentModel.DataAnnotations;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Models;

public class VjezbaCreateViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Naziv vježbe je obavezan.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv mora biti između 2 i 100 znakova.")]
    [Display(Name = "Naziv vježbe")]
    public string Naziv { get; set; } = string.Empty;

    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Opis mora biti između 3 i 1000 znakova.")]
    [Display(Name = "Opis")]
    public string? Opis { get; set; }

    [Required(ErrorMessage = "Odaberite mišićnu skupinu.")]
    [Display(Name = "Mišićna skupina")]
    public MisicnaSkupina MisicnaSkupina { get; set; }

    [Url(ErrorMessage = "Unesite ispravan URL.")]
    [Display(Name = "URL video demonstracije")]
    public string? VideoUrl { get; set; }

    [Required(ErrorMessage = "Razina težine je obavezna.")]
    [Range(1, 5, ErrorMessage = "Razina težine mora biti između 1 i 5.")]
    [Display(Name = "Razina težine (1–5)")]
    public int RazinaTezine { get; set; } = 1;

    [Display(Name = "Potrebna oprema")]
    public bool PotrebnaOprema { get; set; }
}
