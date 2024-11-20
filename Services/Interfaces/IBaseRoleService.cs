using ordreChange.Models;

namespace ordreChange.Services.Interfaces
{
    public interface IBaseRoleService
    {
        Task<T> ValidateAndExecuteAsync<T>(int agentId, int? ordreId, string action, Func<Agent, Task<T>> execute);
    }
}
