using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public class AcheteurStrategy : IRoleStrategy
    {
        public Task<bool> CanExecuteActionAsync(Ordre ordre, int agentId, string action)
        {
            if (action == "Modifier" && ordre.IdAgent != agentId)
                throw new InvalidOperationException("Seul le créateur de cet ordre est autorisé à le modifier.");

            if (ordre.Statut == "Validé")
                throw new InvalidOperationException("L'ordre est déjà validé et ne peut plus être modifié.");

            return Task.FromResult(true);
        }
    }
}
