using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    /// <summary>
    /// Contexte pour exécuter des stratégies en fonction des rôles.
    /// </summary>
    public class RoleStrategyContext
    {
        private readonly Dictionary<string, IRoleStrategy> _strategies;

        public RoleStrategyContext()
        {
            _strategies = new Dictionary<string, IRoleStrategy>();
        }

        /// <summary>
        /// Enregistre une stratégie pour un rôle spécifique.
        /// </summary>
        /// <param name="roleName">Nom du rôle (ex. "Acheteur", "Validateur").</param>
        /// <param name="strategy">Stratégie à associer.</param>
        public void RegisterStrategy(string roleName, IRoleStrategy strategy)
        {
            _strategies[roleName] = strategy;
        }

        /// <summary>
        /// Vérifie si un agent a le droit d'exécuter une action en fonction de son rôle.
        /// </summary>
        /// <param name="roleName">Nom du rôle de l'agent.</param>
        /// <param name="ordre">L'ordre concerné (si applicable).</param>
        /// <param name="agentId">Identifiant de l'agent effectuant l'action.</param>
        /// <param name="action">Action à exécuter.</param>
        public async Task CanExecuteAsync(string roleName, Ordre? ordre, int agentId, string action)
        {
            if (!_strategies.ContainsKey(roleName))
                throw new InvalidOperationException($"Aucune stratégie définie pour le rôle : {roleName}");

            var strategy = _strategies[roleName];
            await strategy.ValidateActionAsync(ordre, agentId, action);
        }
    }
}                              
