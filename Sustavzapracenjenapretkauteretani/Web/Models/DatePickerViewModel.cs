namespace Sustavzapracenjenapretkauteretani.Web.Models
{
    public class DatePickerViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool WithTime { get; set; } = false;
        public string? InitialValue { get; set; }
        public string? Hint { get; set; }
        public string? MinDate { get; set; }
        public string? MaxDate { get; set; }
    }
}
