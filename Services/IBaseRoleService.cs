using ordreChange.Models;

namespace ordreChange.Services
{
    public interface IBaseRoleService
    {
        Task<T> ValidateAndExecuteAsync<T>(int agentId, int? ordreId, string action, Func<Agent, Task<T>> execute);
    }
}
