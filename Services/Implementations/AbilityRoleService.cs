using NLog;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Strategies.Roles;

namespace ordreChange.Services.Implementations
{
    public class AbilityRoleService
    {
        private readonly IAgentRepository _agentRepository;
        private readonly RoleStrategyContext _roleStrategyContext;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AbilityRoleService(IAgentRepository agentRepository, RoleStrategyContext roleStrategyContext)
        {
            _agentRepository = agentRepository;
            _roleStrategyContext = roleStrategyContext;
        }

        /// <summary>
        /// Check permissions
        /// </summary>
        public async Task<Agent> ValidateAgentAndPermissionAsync<TEntity>(int agentId, TEntity? entity, string action)
            where TEntity : class
        {
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new UnauthorizedAccessException("No agent found to perform this action");
            }

            await _roleStrategyContext.CanExecuteAsync(agent, entity, action);
            return agent;
        }
    }
}
