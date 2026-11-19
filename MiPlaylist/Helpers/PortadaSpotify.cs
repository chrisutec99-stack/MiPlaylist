using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace MiPlaylist.Helpers
{
    public static class PortadaSpotify
    {
        // Devuelve la URL de la portada o null si no hay enlace o Spotify falla.
        public static string Obtener(string spotifyId)
        {
            if (string.IsNullOrWhiteSpace(spotifyId)) return null;
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var enlace = "https://open.spotify.com/track/" + spotifyId;
                var url = "https://open.spotify.com/oembed?url=" + Uri.EscapeDataString(enlace);

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 5000;
                req.UserAgent = "MiPlaylist/1.0";

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    var json = new JavaScriptSerializer()
                        .Deserialize<Dictionary<string, object>>(sr.ReadToEnd());

                    object thumb;
                    if (json.TryGetValue("thumbnail_url", out thumb) && thumb != null)
                    {
                        var s = thumb.ToString();
                        return s.Length <= 500 ? s : null;   // la columna admite 500
                    }
                }
            }
            catch
            {
                // Si Spotify falla, la canción simplemente queda sin portada
            }
            return null;
        }
    }
}