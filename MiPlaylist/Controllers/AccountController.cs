using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using MiPlaylist.Helpers;
using MiPlaylist.Models;

namespace MiPlaylist.Controllers
{
    public class AccountController : Controller
    {
        private readonly MiPlaylistContext db = new MiPlaylistContext();

        private const int MinutosValidez = 10;
        private const int MaxIntentos = 5;
        private const int SegundosEntreSolicitudes = 60;

        [HttpGet]
        public ActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(RegistroViewModel modelo)
        {
            // Regla de negocio: el correo no se puede repetir
            if (!string.IsNullOrWhiteSpace(modelo.Correo))
            {
                string correo = modelo.Correo.Trim().ToLower();
                if (db.Usuarios.Any(u => u.Correo == correo))
                    ModelState.AddModelError("Correo", "Ya existe una cuenta con ese correo");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            db.Usuarios.Add(new Usuario
            {
                Nombre = modelo.Nombre.Trim(),
                Correo = modelo.Correo.Trim().ToLower(),
                ClaveHash = Seguridad.HashClave(modelo.Clave)
            });
            db.SaveChanges();

            TempData["Mensaje"] = "Cuenta creada correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Catalogo");
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            string correo = modelo.Correo.Trim().ToLower();
            var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correo);

            // Mismo mensaje si falla el correo o la contraseña (no dar pistas)
            if (usuario == null || !Seguridad.Verificar(modelo.Clave, usuario.ClaveHash))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(modelo);
            }

            Session["UsuarioId"] = usuario.Id;
            Session["UsuarioNombre"] = usuario.Nombre;
            SesionPersistente.Guardar(Response, usuario.Id, usuario.Nombre);
            return RedirectToAction("Index", "Catalogo");
        }

        public ActionResult Logout()
        {
            SesionPersistente.Borrar(Response);
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // ===================== RECUPERAR CONTRASEÑA =====================

        [HttpGet]
        public ActionResult OlvideClave()
        {
            return View(new OlvideClaveViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OlvideClave(OlvideClaveViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            string correo = modelo.Correo.Trim().ToLower();
            var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correo);

            if (usuario != null)
            {
                // Límite: una solicitud por minuto y usuario
                var limite = DateTime.Now.AddSeconds(-SegundosEntreSolicitudes);
                bool reciente = db.RecuperacionesClave.Any(r => r.UsuarioId == usuario.Id && r.Creado > limite);

                if (!reciente)
                {
                    // Al pedir un código nuevo, los anteriores dejan de servir
                    var pendientes = db.RecuperacionesClave.Where(r => r.UsuarioId == usuario.Id && !r.Usado).ToList();
                    foreach (var p in pendientes) p.Usado = true;

                    string codigo = GenerarCodigo();
                    db.RecuperacionesClave.Add(new RecuperacionClave
                    {
                        UsuarioId = usuario.Id,
                        CodigoHash = Seguridad.HashClave(codigo),   // el código NO se guarda en claro
                        Expira = DateTime.Now.AddMinutes(MinutosValidez),
                        Creado = DateTime.Now
                    });
                    db.SaveChanges();

                    string cuerpo =
                        "<div style='font-family:Arial,sans-serif'>" +
                        "<h2>MiPlaylist</h2>" +
                        "<p>Hola " + System.Net.WebUtility.HtmlEncode(usuario.Nombre) + ", recibimos una solicitud para cambiar tu contraseña.</p>" +
                        "<p>Tu código es:</p>" +
                        "<p style='font-size:32px;font-weight:bold;letter-spacing:6px'>" + codigo + "</p>" +
                        "<p>Vence en " + MinutosValidez + " minutos. Si no fuiste tú, ignora este mensaje.</p></div>";

                    ServicioCorreo.Enviar(usuario.Correo, "Tu código para recuperar la contraseña", cuerpo);

                    if (ConfigurationManager.AppSettings["Recuperacion:MostrarCodigoEnPantalla"] == "true")
                        TempData["Mensaje"] = "Modo de pruebas: tu código es " + codigo;
                }
            }

            // Mismo mensaje exista o no el correo (no revelar qué cuentas están registradas)
            if (TempData["Mensaje"] == null)
                TempData["Mensaje"] = "Si el correo está registrado, te enviamos un código de 6 dígitos.";

            return RedirectToAction("RestablecerClave", new { correo = correo });
        }

        [HttpGet]
        public ActionResult RestablecerClave(string correo)
        {
            return View(new RestablecerClaveViewModel { Correo = correo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestablecerClave(RestablecerClaveViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            const string errorGenerico = "El código es incorrecto o ya venció. Pide uno nuevo si hace falta.";
            string correo = modelo.Correo.Trim().ToLower();
            var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
            {
                ModelState.AddModelError("", errorGenerico);
                return View(modelo);
            }

            var ahora = DateTime.Now;
            var rec = db.RecuperacionesClave
                .Where(r => r.UsuarioId == usuario.Id && !r.Usado && r.Expira > ahora && r.Intentos < MaxIntentos)
                .OrderByDescending(r => r.Creado)
                .FirstOrDefault();

            if (rec == null)
            {
                ModelState.AddModelError("", errorGenerico);
                return View(modelo);
            }

            if (!Seguridad.Verificar(modelo.Codigo, rec.CodigoHash))
            {
                rec.Intentos++;
                db.SaveChanges();
                ModelState.AddModelError("", errorGenerico);
                return View(modelo);
            }

            // Código correcto: se cambia la clave y el código queda inservible
            usuario.ClaveHash = Seguridad.HashClave(modelo.ClaveNueva);
            var todos = db.RecuperacionesClave.Where(r => r.UsuarioId == usuario.Id && !r.Usado).ToList();
            foreach (var r in todos) r.Usado = true;
            db.SaveChanges();

            TempData["Mensaje"] = "Contraseña actualizada. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        private static string GenerarCodigo()
        {
            var bytes = new byte[4];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(bytes);
            uint n = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return n.ToString("000000");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}