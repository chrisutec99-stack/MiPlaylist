using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace MiPlaylist.Helpers
{
    public static class ServicioCorreo
    {
        // Devuelve true si el correo salió; false si no está configurado o falló.
        public static bool Enviar(string para, string asunto, string cuerpoHtml)
        {
            string host = ConfigurationManager.AppSettings["Smtp:Host"];
            string usuario = ConfigurationManager.AppSettings["Smtp:Usuario"];
            string clave = ConfigurationManager.AppSettings["Smtp:Clave"];
            int puerto;
            if (!int.TryParse(ConfigurationManager.AppSettings["Smtp:Puerto"], out puerto)) puerto = 587;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario)
                || string.IsNullOrWhiteSpace(clave) || usuario.Contains("CORREO_REMITENTE"))
            {
                Debug.WriteLine("[Correo] SMTP sin configurar (revisa secretos.config).");
                return false;
            }

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(usuario, "MiPlaylist");
                    msg.To.Add(para);
                    msg.Subject = asunto;
                    msg.Body = cuerpoHtml;
                    msg.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(host, puerto))
                    {
                        smtp.EnableSsl = true;
                        smtp.Credentials = new NetworkCredential(usuario, clave);
                        smtp.Timeout = 10000;
                        smtp.Send(msg);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Correo] Error al enviar: " + ex.Message);
                return false;
            }
        }
    }
}