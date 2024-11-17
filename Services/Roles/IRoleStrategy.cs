using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public interface IRoleStrategy
    {
        Task ValidateActionAsync(Ordre? ordre, int agentId, string action);
    }
}
