using ordreChange.Services.Interfaces;

namespace ordreChange.Services.Implementations
{
    public class TauxChangeService : ITauxChangeService
    {
        public float GetTaux(string deviseSource, string deviseCible)
        {
            return (float)TauxChangeMatrix.Instance.GetTaux(deviseSource, deviseCible);
        }
    }
}
