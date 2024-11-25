using ordreChange.Models;

namespace ordreChange.Strategies.Roles
{
    public class AcheteurStrategy : IRoleStrategy
    {
        public Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Création")
            {
                // Les acheteurs peuvent toujours créer un ordre
                return Task.CompletedTask;
            }

            if (action == "Modification" || action == "Annulation")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new UnauthorizedAccessException("Only the creator of this order is authorized to modify it.");
                if (ordre.Statut == "Validé")
                    throw new UnauthorizedAccessException("Order already validated, modification or cancellation not authorized.");
                return Task.CompletedTask;
            }

            if (action == "History")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new UnauthorizedAccessException("Only the creator of this order is authorized to access this history.");
                return Task.CompletedTask;
            }
            throw new UnauthorizedAccessException($"Action '{action}' not allowed for the ACHETEUR role.");
        }
    }
}
