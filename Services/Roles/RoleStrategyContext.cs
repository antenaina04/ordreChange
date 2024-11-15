using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public class RoleStrategyContext
    {
        private readonly Dictionary<Role, IRoleStrategy> _strategies;

        public RoleStrategyContext()
        {
            _strategies = new Dictionary<Role, IRoleStrategy>
            {
                { Role.Acheteur, new AcheteurStrategy() },
                { Role.Validateur, new ValidateurStrategy() }
            };
        }

        public async Task<bool> CanExecuteAsync(Role role, Ordre ordre, int agentId, string action)
        {
            if (!_strategies.ContainsKey(role))
                throw new InvalidOperationException($"Le rôle {role} n'est pas pris en charge.");

            return await _strategies[role].CanExecuteActionAsync(ordre, agentId, action);
        }
    }
}
