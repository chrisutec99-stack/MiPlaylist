using System.ComponentModel.DataAnnotations;

namespace MiPlaylist.Models
{
    public class OlvideClaveViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }
    }

    public class RestablecerClaveViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Escribe el código que te enviamos")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "El código tiene 6 dígitos")]
        [Display(Name = "Código de 6 dígitos")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Debe incluir al menos una mayúscula y un número")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string ClaveNueva { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("ClaveNueva", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarClave { get; set; }
    }
}