using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MiPlaylist.Models
{
    public class ListaCrearViewModel
    {
        [Required(ErrorMessage = "El nombre de la lista es obligatorio")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres")]
        [Display(Name = "Nombre de la lista")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede pasar de 250 caracteres")]
        [Display(Name = "Descripción (opcional)")]
        public string Descripcion { get; set; }
    }

    public class ListaResumen
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Total { get; set; }
        public int DuracionTotalSeg { get; set; }
        public List<string> Portadas { get; set; } = new List<string>();
    }

    public class ListaDetalleViewModel
    {
        public Lista Lista { get; set; }
        public List<Cancion> Canciones { get; set; }
        public List<SelectListItem> Disponibles { get; set; }
        public int DuracionTotalSeg { get; set; }
    }
}