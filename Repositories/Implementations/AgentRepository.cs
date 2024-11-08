using ordreChange.Data;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;

namespace ordreChange.Repositories.Implementations
{
    public class AgentRepository : Repository<Agent>, IAgentRepository
    {
        public AgentRepository(OrdreDeChangeContext context) : base(context)
        {
        }
        // Implémentation spécifique à Agent si nécessaire
    }
}
