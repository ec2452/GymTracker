namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    /// <summary>
    /// ViewModel za reusable autocomplete partial view
    /// </summary>
    public class AutocompleteFieldViewModel
    {
        /// <summary>
        /// Naziv polja (koristi se za ID-ove, name atribut i data-valmsg-for)
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Label tekst
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// API endpoint za pretragu (npr. /korisnici/pretraga)
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Inicijalna vrijednost tekst inputa (za Edit - popunjeni naziv)
        /// </summary>
        public string InitialValue { get; set; } = string.Empty;

        /// <summary>
        /// Inicijalna vrijednost hidden inputa (za Edit - popunjeni ID)
        /// </summary>
        public int? InitialId { get; set; }
    }
}
