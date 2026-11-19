using System.Linq;
using System.Web.Mvc;
using MiPlaylist.Models;
using MiPlaylist.ViewModels;   // AJUSTAR si tus ViewModels están en otro namespace

namespace MiPlaylist.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var usuarioId = Session["UsuarioId"] as int?;
            if (usuarioId == null)
            {
                return View((HomeResumenViewModel)null);
            }

            using (var db = new MiPlaylistContext())
            {
                var id = usuarioId.Value;

                var vm = new HomeResumenViewModel
                {
                    Nombre = (Session["UsuarioNombre"] ?? "").ToString(),
                    TotalBiblioteca = db.UsuarioCanciones.Count(uc => uc.UsuarioId == id),
                    TotalFavoritas = db.UsuarioCanciones.Count(uc => uc.UsuarioId == id && uc.EsFavorita),
                    TotalListas = db.Listas.Count(l => l.UsuarioId == id),
                    TotalCatalogo = db.Canciones.Count()
                };

                vm.Recientes = (from uc in db.UsuarioCanciones
                                join c in db.Canciones on uc.CancionId equals c.Id
                                where uc.UsuarioId == id
                                orderby uc.FechaAgregada descending
                                select new UltimaCancionItem { Titulo = c.Titulo, Artista = c.Artista })
                               .Take(5)
                               .ToList();

                return View(vm);
            }
        }
    }
}