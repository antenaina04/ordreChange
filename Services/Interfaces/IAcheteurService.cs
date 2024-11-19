using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Services.Interfaces
{
    public interface IAcheteurService : IBaseRoleService
    {
        Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible);
        Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto);
    }
}
