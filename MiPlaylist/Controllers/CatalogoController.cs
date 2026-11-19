using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiPlaylist.Helpers;
using MiPlaylist.Models;

namespace MiPlaylist.Controllers
{
    [RequiereLogin]
    public class CatalogoController : Controller
    {
        private readonly MiPlaylistContext db = new MiPlaylistContext();
        private const int PorPagina = 20;

        private int UsuarioActual
        {
            get { return (int)Session["UsuarioId"]; }
        }

        public ActionResult Index(string q, string genero, int pagina = 1)
        {
            var consulta = db.Canciones.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                consulta = consulta.Where(c => c.Titulo.Contains(q) || c.Artista.Contains(q) || c.Album.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(genero))
            {
                consulta = consulta.Where(c => c.Genero == genero);
            }

            int total = consulta.Count();
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)PorPagina));
            pagina = Math.Min(Math.Max(1, pagina), totalPaginas);

            var canciones = consulta.OrderBy(c => c.Titulo)
                                    .Skip((pagina - 1) * PorPagina)
                                    .Take(PorPagina)
                                    .ToList();

            int uid = UsuarioActual;
            var ids = canciones.Select(c => c.Id).ToList();
            var enBiblioteca = new HashSet<int>(
                db.UsuarioCanciones.Where(x => x.UsuarioId == uid && ids.Contains(x.CancionId))
                                   .Select(x => x.CancionId));

            var modelo = new CatalogoViewModel
            {
                Canciones = canciones,
                Generos = db.Canciones.Select(c => c.Genero).Distinct().OrderBy(g => g).ToList(),
                EnBiblioteca = enBiblioteca,
                Q = q,
                Genero = genero,
                Pagina = pagina,
                TotalPaginas = totalPaginas,
                Total = total,
                UsuarioId = uid
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(int id, string q, string genero, int pagina = 1)
        {
            if (db.Canciones.Find(id) == null)
                return HttpNotFound();

            int uid = UsuarioActual;

            if (db.UsuarioCanciones.Any(x => x.UsuarioId == uid && x.CancionId == id))
            {
                TempData["Error"] = "Esa canción ya está en tu biblioteca";
            }
            else
            {
                db.UsuarioCanciones.Add(new UsuarioCancion { UsuarioId = uid, CancionId = id });
                db.SaveChanges();
                TempData["Mensaje"] = "Canción agregada a tu biblioteca";
            }
            return RedirectToAction("Index", new { q, genero, pagina });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            var vm = new CancionCrearViewModel { Generos = ObtenerGeneros() };
            return View(vm);
        }

        private IEnumerable<SelectListItem> ObtenerGeneros()
        {
            return db.Canciones
                     .Select(c => c.Genero)
                     .Distinct()
                     .OrderBy(g => g)
                     .ToList()
                     .Select(g => new SelectListItem { Value = g, Text = g })
                     .ToList();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear(CancionCrearViewModel vm)
        {
            vm.Generos = ObtenerGeneros();

            int segundos = 0;
            if (ModelState.IsValidField("Duracion"))
            {
                segundos = ConvertirDuracion(vm.Duracion);
                if (segundos < 10 || segundos > 3600)
                    ModelState.AddModelError("Duracion", "La duración debe estar entre 0:10 y 60:00.");
            }

            string spotifyId = null;
            if (!string.IsNullOrWhiteSpace(vm.EnlaceSpotify))
            {
                spotifyId = ExtraerSpotifyId(vm.EnlaceSpotify);
                if (spotifyId == null)
                    ModelState.AddModelError("EnlaceSpotify", "Pega el enlace de la canción desde Spotify (Compartir > Copiar enlace).");
            }

            if (db.Canciones.Any(c => c.Titulo == vm.Titulo && c.Artista == vm.Artista))
                ModelState.AddModelError("Titulo", "Ya existe esa canción de ese artista.");

            if (spotifyId != null && db.Canciones.Any(c => c.SpotifyId == spotifyId))
                ModelState.AddModelError("EnlaceSpotify", "Ese enlace de Spotify ya pertenece a otra canción del catálogo.");

            if (!ModelState.IsValid) return View(vm);

            db.Canciones.Add(new Cancion
            {
                Titulo = vm.Titulo.Trim(),
                Artista = vm.Artista.Trim(),
                Genero = vm.Genero,
                Anio = vm.Anio,
                DuracionSeg = segundos,
                SpotifyId = spotifyId,
                PortadaUrl = PortadaSpotify.Obtener(spotifyId),   // null si no hay enlace o falla
                CreadaPorId = UsuarioActual
            });
            db.SaveChanges();

            TempData["Mensaje"] = "Canción creada.";
            return RedirectToAction("Index");
        }

        private static int ConvertirDuracion(string texto)
        {
            var partes = texto.Split(':');
            return int.Parse(partes[0]) * 60 + int.Parse(partes[1]);
        }

        private static string ExtraerSpotifyId(string texto)
        {
            texto = texto.Trim();
            var m = System.Text.RegularExpressions.Regex.Match(texto, @"track/([A-Za-z0-9]{22})");
            if (m.Success) return m.Groups[1].Value;
            if (System.Text.RegularExpressions.Regex.IsMatch(texto, @"^[A-Za-z0-9]{22}$")) return texto;
            return null;
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var c = db.Canciones.Find(id);
            if (c == null || c.CreadaPorId != UsuarioActual)
                return HttpNotFound();

            var vm = new CancionEditarViewModel
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Artista = c.Artista,
                Genero = c.Genero,
                Anio = c.Anio,
                Duracion = c.DuracionSeg.HasValue
                    ? (c.DuracionSeg.Value / 60) + ":" + (c.DuracionSeg.Value % 60).ToString("00")
                    : "",
                EnlaceSpotify = string.IsNullOrEmpty(c.SpotifyId)
                    ? "" : "https://open.spotify.com/track/" + c.SpotifyId,
                Generos = ObtenerGeneros()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(CancionEditarViewModel vm)
        {
            var c = db.Canciones.Find(vm.Id);
            if (c == null || c.CreadaPorId != UsuarioActual)
                return HttpNotFound();

            vm.Generos = ObtenerGeneros();

            int segundos = 0;
            if (ModelState.IsValidField("Duracion"))
            {
                segundos = ConvertirDuracion(vm.Duracion);
                if (segundos < 10 || segundos > 3600)
                    ModelState.AddModelError("Duracion", "La duración debe estar entre 0:10 y 60:00.");
            }

            string spotifyId = null;
            if (!string.IsNullOrWhiteSpace(vm.EnlaceSpotify))
            {
                spotifyId = ExtraerSpotifyId(vm.EnlaceSpotify);
                if (spotifyId == null)
                    ModelState.AddModelError("EnlaceSpotify", "Pega el enlace de la canción desde Spotify (Compartir > Copiar enlace).");
            }

            if (db.Canciones.Any(x => x.Id != vm.Id && x.Titulo == vm.Titulo && x.Artista == vm.Artista))
                ModelState.AddModelError("Titulo", "Ya existe esa canción de ese artista.");

            if (spotifyId != null && db.Canciones.Any(x => x.Id != vm.Id && x.SpotifyId == spotifyId))
                ModelState.AddModelError("EnlaceSpotify", "Ese enlace de Spotify ya pertenece a otra canción del catálogo.");

            if (!ModelState.IsValid) return View(vm);

            // Si cambió el enlace (o antes no tenía portada), se vuelve a buscar
            if (spotifyId != c.SpotifyId || string.IsNullOrEmpty(c.PortadaUrl))
                c.PortadaUrl = PortadaSpotify.Obtener(spotifyId);

            c.Titulo = vm.Titulo.Trim();
            c.Artista = vm.Artista.Trim();
            c.Genero = vm.Genero;
            c.Anio = vm.Anio;
            c.DuracionSeg = segundos;
            c.SpotifyId = spotifyId;
            db.SaveChanges();

            TempData["Mensaje"] = "Canción actualizada.";
            return RedirectToAction("Index");
        }
    }
}