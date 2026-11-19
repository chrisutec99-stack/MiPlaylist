using System.Collections.Generic;

namespace MiPlaylist.Models
{
    public class BibliotecaViewModel
    {
        public List<UsuarioCancion> Items { get; set; }
        public List<string> Generos { get; set; }
        public string Q { get; set; }
        public string Genero { get; set; }
        public bool SoloFavoritas { get; set; }
        public int TotalBiblioteca { get; set; }
        public int TotalFavoritas { get; set; }
    }

    public class CreadasViewModel
    {
        public List<Cancion> Canciones { get; set; }
        public HashSet<int> EnBiblioteca { get; set; }
    }
}