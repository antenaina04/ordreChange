using ordreChange.Models;

namespace ordreChange.Services.Interfaces
{
    public interface IAcheteurService : IBaseRoleService
    {
        Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible);
    }
}
