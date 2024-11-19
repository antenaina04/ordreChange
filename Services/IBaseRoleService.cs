using ordreChange.Models;

namespace ordreChange.Services
{
    public interface IBaseRoleService
    {
        Task<T> ValidateAndExecuteAsync<T>(int agentId, string action, Func<Agent, Task<T>> execute);
    }
}
