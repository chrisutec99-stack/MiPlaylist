using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MiPlaylist.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido, por ejemplo nombre@correo.com")]
        [StringLength(120, ErrorMessage = "El correo no puede pasar de 120 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Debe incluir al menos una mayúscula y un número")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("Clave", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarClave { get; set; }
    }
}