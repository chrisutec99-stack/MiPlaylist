using System;
using System.Security.Cryptography;

namespace MiPlaylist.Helpers
{
    public static class Seguridad
    {
        private const int Iteraciones = 100000;

        public static string HashClave(string clave)
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);

            byte[] hash;
            using (var kdf = new Rfc2898DeriveBytes(clave, salt, Iteraciones, HashAlgorithmName.SHA256))
                hash = kdf.GetBytes(32);

            return Iteraciones + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string clave, string almacenado)
        {
            var partes = almacenado.Split('.');
            if (partes.Length != 3) return false;

            int iter = int.Parse(partes[0]);
            var salt = Convert.FromBase64String(partes[1]);
            var esperado = Convert.FromBase64String(partes[2]);

            byte[] hash;
            using (var kdf = new Rfc2898DeriveBytes(clave, salt, iter, HashAlgorithmName.SHA256))
                hash = kdf.GetBytes(esperado.Length);

            int diferencia = 0;
            for (int i = 0; i < esperado.Length; i++) diferencia |= hash[i] ^ esperado[i];
            return diferencia == 0;
        }
    }
}