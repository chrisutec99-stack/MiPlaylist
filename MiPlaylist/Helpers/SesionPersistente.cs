using System;
using System.Web;
using System.Web.Security;
using MiPlaylist.Models;

namespace MiPlaylist.Helpers
{
    public static class SesionPersistente
    {
        private const string NombreCookie = "MiPlaylistRecordar";
        private const int Dias = 7;

        // Llamar justo después de un login correcto
        public static void Guardar(HttpResponseBase respuesta, int usuarioId, string nombre)
        {
            var ahora = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
                1, usuarioId.ToString(), ahora, ahora.AddDays(Dias), true, nombre ?? "");

            respuesta.Cookies.Add(new HttpCookie(NombreCookie, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,                 // JavaScript no puede leerla
                Expires = ahora.AddDays(Dias)
            });
        }

        // Llamar en el logout
        public static void Borrar(HttpResponseBase respuesta)
        {
            respuesta.Cookies.Add(new HttpCookie(NombreCookie, "")
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(-1)
            });
        }

        // Se llama cuando el servidor crea una sesión nueva
        public static void Restaurar(HttpContext ctx)
        {
            try
            {
                var cookie = ctx.Request.Cookies[NombreCookie];
                if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return;

                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket == null || ticket.Expired) return;

                int id;
                if (!int.TryParse(ticket.Name, out id)) return;

                using (var db = new MiPlaylistContext())
                {
                    var u = db.Usuarios.Find(id);     // el usuario debe seguir existiendo
                    if (u == null) return;
                    ctx.Session["UsuarioId"] = u.Id;
                    ctx.Session["UsuarioNombre"] = u.Nombre;
                }
            }
            catch
            {
                // Cookie manipulada o inválida: se ignora y el usuario inicia sesión normal
            }
        }
    }
}