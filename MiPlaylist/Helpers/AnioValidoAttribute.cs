using System;
using System.ComponentModel.DataAnnotations;

namespace MiPlaylist.Helpers
{
    public class AnioValidoAttribute : ValidationAttribute
    {
        public int Minimo { get; set; } = 1900;

        public AnioValidoAttribute()
        {
            ErrorMessage = "El año debe estar entre {0} y {1}.";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true; // [Required] se encarga de lo vacío
            int anio;
            if (!int.TryParse(value.ToString(), out anio)) return false;
            return anio >= Minimo && anio <= DateTime.Now.Year + 1;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessage, Minimo, DateTime.Now.Year + 1);
        }
    }
}