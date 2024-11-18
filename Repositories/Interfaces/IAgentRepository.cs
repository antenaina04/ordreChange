using ordreChange.Models;

namespace ordreChange.Repositories.Interfaces
{
    public interface IAgentRepository : IRepository<Agent>
    {
        Task<Agent?> GetByUsernameAsync(string username);
        new Task<Agent?> GetByIdAsync(int id);
    }
}
