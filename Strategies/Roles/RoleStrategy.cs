using ordreChange.Models;
using System;
using System.Threading.Tasks;

namespace ordreChange.Strategies.Roles
{
    
    #region Acheteur Ordre
    public class OrdreAcheteurStrategy : IRoleStrategy<Ordre>
    {
        public async Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            switch (action)
            {
                case "Création":
                    await Task.CompletedTask;
                    return;

                case "Modification":
                case "Annulation":
                    if (ordre.IdAgent != agentId)
                        throw new UnauthorizedAccessException("Only the creator of this order is authorized to modify it.");
                    if (ordre.Statut == "Annulé")
                        throw new UnauthorizedAccessException("Order already canceled, modification or cancellation not authorized.");
                    if (ordre.Statut == "Validé")
                        throw new UnauthorizedAccessException("Order already validated, modification or cancellation not authorized.");

                    await Task.CompletedTask;
                    return;

                case "History":
                case "GET_ORDRE":
                    if (ordre == null || ordre.IdAgent != agentId)
                        throw new UnauthorizedAccessException("Only the creator of this order is authorized to access this history.");

                    await Task.CompletedTask;
                    return;

                default:
                    throw new UnauthorizedAccessException($"Action '{action}' not allowed for the ACHETEUR role.");
            }
        }
    }
    #endregion

    #region Validateur Ordre
    public class OrdreValidateurStrategy : IRoleStrategy<Ordre>
    {
        public async Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            switch (action)
            {
                case "Validation":
                case "Refus":
                    if (ordre.Statut != "En attente")
                        throw new UnauthorizedAccessException($"Only pending orders can be {action.ToLower()}.");

                    await Task.CompletedTask;
                    return;

                case "Stats":
                case "History":
                case "Statut":
                case "GET_ORDRE":
                    await Task.CompletedTask;
                    return;

                default:
                    throw new UnauthorizedAccessException($"Action '{action}' not allowed for the VALIDATEUR role.");
            }
        }
    }
    #endregion
    
}
