using MiPlaylist.Helpers;
using MiPlaylist.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MiPlaylist.Controllers
{
    [RequiereLogin]
    public class BibliotecaController : Controller
    {
        private readonly MiPlaylistContext db = new MiPlaylistContext();

        private int UsuarioActual
        {
            get { return (int)Session["UsuarioId"]; }
        }

        public ActionResult Index(string q, string genero, bool soloFavoritas = false)
        {
            int uid = UsuarioActual;

            var propias = db.UsuarioCanciones.Include(x => x.Cancion).Where(x => x.UsuarioId == uid);
            int totalBiblioteca = propias.Count();
            int totalFavoritas = propias.Count(x => x.EsFavorita);
            var generos = propias.Select(x => x.Cancion.Genero).Distinct().OrderBy(g => g).ToList();

            var consulta = propias;
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                consulta = consulta.Where(x => x.Cancion.Titulo.Contains(q)
                                            || x.Cancion.Artista.Contains(q)
                                            || x.Cancion.Album.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(genero))
                consulta = consulta.Where(x => x.Cancion.Genero == genero);
            if (soloFavoritas)
                consulta = consulta.Where(x => x.EsFavorita);

            var modelo = new BibliotecaViewModel
            {
                Items = consulta.OrderByDescending(x => x.FechaAgregada).ToList(),
                Generos = generos,
                Q = q,
                Genero = genero,
                SoloFavoritas = soloFavoritas,
                TotalBiblioteca = totalBiblioteca,
                TotalFavoritas = totalFavoritas
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Favorita(int id, string q, string genero, bool soloFavoritas = false)
        {
            int uid = UsuarioActual;
            var item = db.UsuarioCanciones.FirstOrDefault(x => x.UsuarioId == uid && x.CancionId == id);
            if (item == null)
                return HttpNotFound();     // solo puedes tocar canciones de TU biblioteca

            item.EsFavorita = !item.EsFavorita;
            db.SaveChanges();
            return RedirectToAction("Index", new { q, genero, soloFavoritas });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Quitar(int id, string q, string genero, bool soloFavoritas = false)
        {
            int uid = UsuarioActual;
            var item = db.UsuarioCanciones.FirstOrDefault(x => x.UsuarioId == uid && x.CancionId == id);
            if (item == null)
                return HttpNotFound();

            // Regla de negocio: no quitar una canción que esté en alguna de tus listas
            bool enAlgunaLista = db.ListaCanciones.Any(lc => lc.CancionId == id && lc.Lista.UsuarioId == uid);
            if (enAlgunaLista)
            {
                TempData["Error"] = "No puedes quitar esta canción porque está en una de tus listas. Sácala de la lista primero.";
            }
            else
            {
                db.UsuarioCanciones.Remove(item);
                db.SaveChanges();
                TempData["Mensaje"] = "Canción quitada de tu biblioteca";
            }
            return RedirectToAction("Index", new { q, genero, soloFavoritas });
        }
        public ActionResult Creadas()
        {
            int uid = UsuarioActual;

            // Solo las canciones creadas por MI usuario
            var mias = db.Canciones.Where(c => c.CreadaPorId == uid)
                                   .OrderBy(c => c.Titulo)
                                   .ToList();

            var ids = mias.Select(c => c.Id).ToList();
            var enBiblioteca = new HashSet<int>(
                db.UsuarioCanciones.Where(x => x.UsuarioId == uid && ids.Contains(x.CancionId))
                                   .Select(x => x.CancionId));

            return View(new CreadasViewModel { Canciones = mias, EnBiblioteca = enBiblioteca });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}