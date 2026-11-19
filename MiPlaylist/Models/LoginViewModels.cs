using System.ComponentModel.DataAnnotations;

namespace MiPlaylist.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }
    }
}