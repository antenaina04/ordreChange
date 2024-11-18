using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public class ValidateurStrategy : IRoleStrategy
    {
        public Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Validation")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new InvalidOperationException("Seuls les ordres en attente peuvent être validés.");
                return Task.CompletedTask;
            }

            if (action == "Refus")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new InvalidOperationException("Seuls les ordres en attente peuvent être refusés.");
                return Task.CompletedTask;
            }
            if (action == "Stats" || action == "History" || action == "Statut")
            {
                return Task.CompletedTask;
            }

            throw new InvalidOperationException($"Action '{action}' non autorisée pour le rôle Validateur.");
        }
    }
}
