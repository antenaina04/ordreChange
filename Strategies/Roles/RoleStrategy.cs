using ordreChange.Models;
using System;
using System.Threading.Tasks;

namespace ordreChange.Strategies.Roles
{
    public class OrdreAcheteurStrategy : IRoleStrategy<Ordre>
    {
        public async Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Création")
            {
                await Task.CompletedTask;
                return; 

            }

            if (action == "Modification" || action == "Annulation")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new UnauthorizedAccessException("Only the creator of this order is authorized to modify it.");
                if (ordre.Statut == "Annulé")
                    throw new UnauthorizedAccessException("Order already canceled, modification or cancellation not authorized.");
                if (ordre.Statut == "Validé")
                    throw new UnauthorizedAccessException("Order already validated, modification or cancellation not authorized.");
                await Task.CompletedTask;
                return; 

            }

            if (action == "History")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new UnauthorizedAccessException("Only the creator of this order is authorized to access this history.");
                await Task.CompletedTask;
                return; 

            }

            if (action == "GET_ORDRE")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new UnauthorizedAccessException("Only the creator of this order is authorized to access this history.");
                await Task.CompletedTask;
                return; 

            }
            throw new UnauthorizedAccessException($"Action '{action}' not allowed for the ACHETEUR role.");
        }
    }

    public class OrdreValidateurStrategy : IRoleStrategy<Ordre>
    {
        public async Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Validation")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new UnauthorizedAccessException("Only pending orders can be validated.");
                await Task.CompletedTask;
                return; 

            }

            if (action == "Refus")
            {
                if (ordre == null || ordre.Statut != "En attente")
                    throw new UnauthorizedAccessException("Only pending orders can be refused.");
                await Task.CompletedTask;
                return; 

            }
            if (action == "Stats" || action == "History" || action == "Statut" || action == "GET_ORDRE")
            {
                await Task.CompletedTask;
                return; 

            }

            throw new UnauthorizedAccessException($"Action {action} not allowed for the VALIDATEUR role.");
        }
    }
}