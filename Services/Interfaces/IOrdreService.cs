using ordreChange.Models;

namespace ordreChange.Services.Interfaces
{
    public interface IOrdreService
    {
        Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible);
        Task<Ordre?> GetOrdreByIdAsync(int id);
        Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
        Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string statut);
        Task<bool> ModifierOrdreAsync(Ordre ordre);
    }
}
