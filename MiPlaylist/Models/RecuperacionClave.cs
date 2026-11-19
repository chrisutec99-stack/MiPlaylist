using System;

namespace MiPlaylist.Models
{
    public class RecuperacionClave
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string CodigoHash { get; set; }
        public DateTime Expira { get; set; }
        public int Intentos { get; set; }
        public bool Usado { get; set; }
        public DateTime Creado { get; set; }
    }
}