using ordreChange.Models;

namespace ordreChange.Services.Interfaces
{
    public interface IAbilityRoleService
    {
        Task<Agent> ValidateAgentAndPermissionAsync<TEntity>(int agentId, TEntity? entity, string action) where TEntity : class;
    }
}
