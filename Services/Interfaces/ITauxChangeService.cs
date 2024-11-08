namespace ordreChange.Services.Interfaces
{
    public interface ITauxChangeService
    {
        /// <summary>
        /// Récupère le taux de change entre deux devises.
        /// </summary>
        /// <param name="deviseSource">La devise source (ex : "USD").</param>
        /// <param name="deviseCible">La devise cible (ex : "EUR").</param>
        /// <returns>Le taux de change en float.</returns>
        float GetTaux(string deviseSource, string deviseCible);
    }
}
