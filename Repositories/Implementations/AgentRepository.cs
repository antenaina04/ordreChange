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
        public new async Task<Agent?> GetByIdAsync(int id)
        {
            return await _context.Agents
                .Include(a => a.Role) // Chargement eager de la relation Role
                .FirstOrDefaultAsync(a => a.IdAgent == id);
        }
        public async Task<Agent?> GetByUsernameAsync(string username)
        {
            return await _context.Agents
                .Include(a => a.Role) // Eager loading de la relation Role
                .FirstOrDefaultAsync(a => a.Username == username);
        }
    }
}
