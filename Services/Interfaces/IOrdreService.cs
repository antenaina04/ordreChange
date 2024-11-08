using ordreChange.Models;

namespace ordreChange.Services.Interfaces
{
    public interface IOrdreService
    {
        Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise);
        Task<bool> ValiderOrdreAsync(int ordreId);
        Task<bool> ModifierOrdreAsync(Ordre ordre);
    }
}
