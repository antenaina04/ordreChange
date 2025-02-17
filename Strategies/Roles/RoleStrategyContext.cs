using ordreChange.Models;

namespace ordreChange.Strategies.Roles
{
    /// <summary>
    /// Context to execute stragegy by roleName
    /// </summary>
    public class RoleStrategyContext
    {
        private readonly Dictionary<string, object> _strategies;

        public RoleStrategyContext()
        {
            _strategies = new Dictionary<string, object>();
        }

        /// <summary>
        /// Enregistre une stratégie pour un rôle spécifique.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <param name="strategy">Strategy to be associated</param>
        public void RegisterStrategy<TEntity>(string roleName, IRoleStrategy<TEntity> strategy)
             where TEntity : class
        {
            _strategies[roleName] = strategy;
        }

        /// <summary>
        /// check si un agent peut exécuter une action sur une entité by his role
        /// </summary>
        # region Check user permission
        public async Task CanExecuteAsync<TEntity>(Agent agent, TEntity? entity, string action)
         where TEntity : class
        {
            if (!_strategies.ContainsKey(agent.Role.Name))
                throw new InvalidOperationException($"Aucune stratégie définie pour le rôle : {agent.Role.Name}");

            var strategy = _strategies[agent.Role.Name] as dynamic; // Convert to dynamic

            await strategy.ValidateActionAsync((dynamic)entity, agent.IdAgent, action);
          
        }
        #endregion
    }
}
