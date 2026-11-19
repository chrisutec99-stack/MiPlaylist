using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiPlaylist.Helpers;
using MiPlaylist.Models;

namespace MiPlaylist.Controllers
{
    [RequiereLogin]
    public class ListasController : Controller
    {
        private readonly MiPlaylistContext db = new MiPlaylistContext();

        private int UsuarioActual
        {
            get { return (int)Session["UsuarioId"]; }
        }

        // Devuelve la lista solo si pertenece al usuario actual
        private Lista ObtenerMiLista(int id)
        {
            int uid = UsuarioActual;
            return db.Listas.FirstOrDefault(l => l.Id == id && l.UsuarioId == uid);
        }

        public ActionResult Index()
        {
            int uid = UsuarioActual;
            var listas = db.Listas
                .Where(l => l.UsuarioId == uid)
                .OrderBy(l => l.Nombre)
                .Select(l => new ListaResumen
                {
                    Id = l.Id,
                    Nombre = l.Nombre,
                    Descripcion = l.Descripcion,
                    Total = l.Canciones.Count(),
                    DuracionTotalSeg = l.Canciones.Sum(lc => (int?)lc.Cancion.DuracionSeg) ?? 0
                }).ToList();

            // Portadas para el mosaico: una sola consulta para todas mis listas
            var ids = listas.Select(l => l.Id).ToList();
            var portadas = db.ListaCanciones
                .Where(lc => ids.Contains(lc.ListaId)
                          && lc.Cancion.PortadaUrl != null && lc.Cancion.PortadaUrl != "")
                .OrderBy(lc => lc.Cancion.Titulo)
                .Select(lc => new { lc.ListaId, lc.Cancion.PortadaUrl })
                .ToList();

            foreach (var l in listas)
            {
                l.Portadas = portadas.Where(p => p.ListaId == l.Id)
                                     .Select(p => p.PortadaUrl)
                                     .Distinct()
                                     .Take(4)
                                     .ToList();
            }
            return View(listas);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View(new ListaCrearViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(ListaCrearViewModel modelo)
        {
            int uid = UsuarioActual;

            // Regla de negocio: nombre único por usuario
            if (!string.IsNullOrWhiteSpace(modelo.Nombre))
            {
                string nombre = modelo.Nombre.Trim();
                if (db.Listas.Any(l => l.UsuarioId == uid && l.Nombre == nombre))
                    ModelState.AddModelError("Nombre", "Ya tienes una lista con ese nombre");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            var lista = new Lista
            {
                UsuarioId = uid,
                Nombre = modelo.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim()
            };
            db.Listas.Add(lista);
            db.SaveChanges();

            TempData["Mensaje"] = "Lista creada. Ahora agrégale canciones.";
            return RedirectToAction("Detalle", new { id = lista.Id });
        }

        public ActionResult Detalle(int id)
        {
            var lista = ObtenerMiLista(id);
            if (lista == null) return HttpNotFound();

            int uid = UsuarioActual;

            var canciones = db.ListaCanciones
                .Where(lc => lc.ListaId == id)
                .Select(lc => lc.Cancion)
                .OrderBy(c => c.Titulo)
                .ToList();

            var enLista = db.ListaCanciones.Where(lc => lc.ListaId == id).Select(lc => lc.CancionId);
            var disponibles = db.UsuarioCanciones
                .Where(x => x.UsuarioId == uid && !enLista.Contains(x.CancionId))
                .Select(x => x.Cancion)
                .OrderBy(c => c.Titulo)
                .ToList()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Titulo + " — " + c.Artista })
                .ToList();

            return View(new ListaDetalleViewModel
            {
                Lista = lista,
                Canciones = canciones,
                Disponibles = disponibles,
                DuracionTotalSeg = canciones.Sum(c => c.DuracionSeg ?? 0)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarCancion(int listaId, int? cancionId)
        {
            var lista = ObtenerMiLista(listaId);
            if (lista == null) return HttpNotFound();

            int uid = UsuarioActual;

            if (cancionId == null)
            {
                TempData["Error"] = "Elige una canción para agregar";
            }
            // Regla: solo canciones que estén en TU biblioteca
            else if (!db.UsuarioCanciones.Any(x => x.UsuarioId == uid && x.CancionId == cancionId.Value))
            {
                TempData["Error"] = "Solo puedes agregar canciones que estén en tu biblioteca";
            }
            // Regla: no repetir la canción dentro de la misma lista
            else if (db.ListaCanciones.Any(lc => lc.ListaId == listaId && lc.CancionId == cancionId.Value))
            {
                TempData["Error"] = "Esa canción ya está en la lista";
            }
            else
            {
                db.ListaCanciones.Add(new ListaCancion { ListaId = listaId, CancionId = cancionId.Value });
                db.SaveChanges();
                TempData["Mensaje"] = "Canción agregada a la lista";
            }
            return RedirectToAction("Detalle", new { id = listaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarCancion(int listaId, int cancionId)
        {
            var lista = ObtenerMiLista(listaId);
            if (lista == null) return HttpNotFound();

            var item = db.ListaCanciones.FirstOrDefault(lc => lc.ListaId == listaId && lc.CancionId == cancionId);
            if (item != null)
            {
                db.ListaCanciones.Remove(item);
                db.SaveChanges();
                TempData["Mensaje"] = "Canción quitada de la lista";
            }
            return RedirectToAction("Detalle", new { id = listaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            var lista = ObtenerMiLista(id);
            if (lista == null) return HttpNotFound();

            // Las filas de ListaCancion se borran solas (ON DELETE CASCADE en la base de datos)
            db.Listas.Remove(lista);
            db.SaveChanges();
            TempData["Mensaje"] = "Lista eliminada";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}