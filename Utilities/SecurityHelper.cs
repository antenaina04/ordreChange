using System;
using System.Security.Cryptography;

namespace ordreChange.Utilities
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Génèration d'une clé sécurisée de longueur spécifiée en utilisant un générateur de nombres aléatoires sécurisé.
        /// </summary>
        /// <param name="length">La longueur en octets de la clé à générer.</param>
        /// <returns>Une chaîne de caractères Base64 représentant la clé sécurisée.</returns>
        public static string GenerateSecureKey(int length)
        {
            var byteArray = new byte[length];
            RandomNumberGenerator.Fill(byteArray);
            return Convert.ToBase64String(byteArray);
        }
    }
}