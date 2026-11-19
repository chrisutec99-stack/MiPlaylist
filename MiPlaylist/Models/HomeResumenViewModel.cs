using System.Collections.Generic;

namespace MiPlaylist.ViewModels   
{
    public class HomeResumenViewModel
    {
        public string Nombre { get; set; }
        public int TotalBiblioteca { get; set; }
        public int TotalFavoritas { get; set; }
        public int TotalListas { get; set; }
        public int TotalCatalogo { get; set; }
        public List<UltimaCancionItem> Recientes { get; set; } = new List<UltimaCancionItem>();
    }

    public class UltimaCancionItem
    {
        public string Titulo { get; set; }
        public string Artista { get; set; }
    }
}