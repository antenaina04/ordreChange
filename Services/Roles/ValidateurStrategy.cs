using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public class ValidateurStrategy : IRoleStrategy
    {
        public Task<bool> CanExecuteActionAsync(Ordre ordre, int agentId, string action)
        {
            if (action == "Valider" && ordre.Statut != "En attente")
                throw new InvalidOperationException("Seuls les ordres en attente peuvent être validés.");

            return Task.FromResult(true);
        }
    }
}
