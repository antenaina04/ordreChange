using ordreChange.Models;

namespace ordreChange.Strategies.Roles
{
    public class ValidateurStrategy : IRoleStrategy
    {
        public Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Validation")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new UnauthorizedAccessException("Only pending orders can be validated.");
                return Task.CompletedTask;
            }

            if (action == "Refus")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new UnauthorizedAccessException("Only pending orders can be refused.");
                return Task.CompletedTask;
            }
            if (action == "Stats" || action == "History" || action == "Statut")
            {
                return Task.CompletedTask;
            }

            throw new UnauthorizedAccessException($"Action {action} not allowed for the VALIDATEUR role.");
        }
    }
}
