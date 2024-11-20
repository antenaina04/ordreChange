using ordreChange.Models;

namespace ordreChange.Strategies.Roles
{
    public interface IRoleStrategy
    {
        Task ValidateActionAsync(Ordre? ordre, int agentId, string action);
    }
}
