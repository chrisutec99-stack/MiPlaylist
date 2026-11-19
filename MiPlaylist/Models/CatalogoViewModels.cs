using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiPlaylist.Models
{
    public class CatalogoViewModel
    {
        public List<Cancion> Canciones { get; set; }
        public List<string> Generos { get; set; }
        public HashSet<int> EnBiblioteca { get; set; }
        public string Q { get; set; }
        public string Genero { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
        public int Total { get; set; }

        public int UsuarioId { get; set; }


    }

    public class CancionCrearViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Entre 2 y 150 caracteres")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El artista es obligatorio")]
        [StringLength(150)]
        public string Artista { get; set; }

        [Required(ErrorMessage = "Elige un género")]
        public string Genero { get; set; }

        [Display(Name = "Año (opcional)")]
        [MiPlaylist.Helpers.AnioValido]
        public int? Anio { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        [RegularExpression(@"^\d{1,3}:[0-5]\d$", ErrorMessage = "Usa el formato minutos:segundos, por ejemplo 3:45")]
        [Display(Name = "Duración (m:ss)")]
        public string Duracion { get; set; }

        [Display(Name = "Enlace de Spotify (opcional)")]
        public string EnlaceSpotify { get; set; }

        public IEnumerable<SelectListItem> Generos { get; set; }
    }

    public class CancionEditarViewModel : CancionCrearViewModel
    {
        public int Id { get; set; }
    }
}