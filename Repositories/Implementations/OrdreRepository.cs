using ordreChange.Data;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;

namespace ordreChange.Repositories.Implementations
{
    public class OrdreRepository : Repository<Ordre>, IOrdreRepository
    {
        public OrdreRepository(OrdreDeChangeContext context) : base(context)
        {
        }
        // Implémentation spécifique à Ordre si nécessaire
    }
}
