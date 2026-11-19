namespace MiPlaylist.Helpers
{
    public static class Formato
    {
        // Para una canción: 3:45
        public static string Duracion(int totalSeg)
        {
            int h = totalSeg / 3600, m = (totalSeg % 3600) / 60, s = totalSeg % 60;
            return h > 0 ? h + ":" + m.ToString("00") + ":" + s.ToString("00")
                         : m + ":" + s.ToString("00");
        }

        // Para una lista completa: "1 h 05 min" o "42 min"
        public static string DuracionTotal(int totalSeg)
        {
            int h = totalSeg / 3600, m = (totalSeg % 3600) / 60;
            if (h > 0) return h + " h " + m.ToString("00") + " min";
            return m + " min";
        }
    }
}