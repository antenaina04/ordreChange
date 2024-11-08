using ordreChange.Services.Interfaces;

namespace ordreChange.Services.Implementations
{
    public class TauxChangeService : ITauxChangeService
    {
        /// <summary>
        /// Simule la récupération du taux de change entre deux devises.
        /// </summary>
        /// <param name="deviseSource">La devise source (ex : "USD").</param>
        /// <param name="deviseCible">La devise cible (ex : "EUR").</param>
        /// <returns>Un taux de change simulé en float.</returns>
        /// 

        /*
            Taux de change aléatoire entre 0.8 et 1.5 lorsque les devises sont différentes (sinon, il retourne 1.0). 
            Pour tester la fonctionnalité sans utiliser un service externe réel
         */
        public float GetTaux(string deviseSource, string deviseCible)
        {
            // Simule le taux de change avec une valeur fixe ou aléatoire
            if (deviseSource == deviseCible)
                return 1.0f;

            // Ici, on peut coder une logique fixe ou aléatoire
            // pour des tests sans appel API réelle.
            Random rand = new Random();
            return (float)(rand.NextDouble() * (1.5 - 0.8) + 0.8);
        }
    }
}
