using Microsoft.EntityFrameworkCore;
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
        public async Task<Agent?> GetByUsernameAsync(string username)
        {
            return await _context.Agents.FirstOrDefaultAsync(a => a.Username == username);
        }
    }
}
