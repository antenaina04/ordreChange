using ordreChange.Models;

namespace ordreChange.Strategies.Roles
{
    public interface IRoleStrategy<TEntity>
     where TEntity : class
    {
        Task ValidateActionAsync(TEntity entity, int agentId, string action);
    }
}
