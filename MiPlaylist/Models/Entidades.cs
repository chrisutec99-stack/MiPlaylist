using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiPlaylist.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required, StringLength(80)] public string Nombre { get; set; }
        [Required, StringLength(120)] public string Correo { get; set; }
        [Required, StringLength(200)] public string ClaveHash { get; set; }
    }

    public class Cancion
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio"), StringLength(200)]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "El artista es obligatorio"), StringLength(200)]
        public string Artista { get; set; }
        [StringLength(200)] public string Album { get; set; }
        [Required, StringLength(60)] public string Genero { get; set; }
        [StringLength(100)] public string Subgenero { get; set; }
        public int? DuracionSeg { get; set; }
        public int? Anio { get; set; }
        [StringLength(30)] public string SpotifyId { get; set; }
        [StringLength(500)] public string PortadaUrl { get; set; }
        public int? CreadaPorId { get; set; }
    }

    public class UsuarioCancion
    {
        public int UsuarioId { get; set; }
        public int CancionId { get; set; }
        public bool EsFavorita { get; set; }
        public DateTime FechaAgregada { get; set; } = DateTime.Now;
        public virtual Usuario Usuario { get; set; }
        public virtual Cancion Cancion { get; set; }
    }

    public class Lista
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Required, StringLength(80)] public string Nombre { get; set; }
        [StringLength(250)] public string Descripcion { get; set; }
        public virtual ICollection<ListaCancion> Canciones { get; set; }
    }

    public class ListaCancion
    {
        public int ListaId { get; set; }
        public int CancionId { get; set; }
        public virtual Lista Lista { get; set; }
        public virtual Cancion Cancion { get; set; }
    }
}