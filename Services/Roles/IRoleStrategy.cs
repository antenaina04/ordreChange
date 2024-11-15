using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public interface IRoleStrategy
    {
        Task<bool> CanExecuteActionAsync(Ordre ordre, int agentId, string action);
    }
}
