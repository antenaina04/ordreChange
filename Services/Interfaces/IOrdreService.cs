using ordreChange.Controllers;
using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Services.Interfaces
{
    public interface IOrdreService
    {
        Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible);
        Task<Ordre?> GetOrdreByIdAsync(int id);
        Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
        Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string statut);
        //Task<bool> ModifierOrdreAsync(int ordreId, int agentId, Ordre ordreModifications);
        Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto ordreModifications);
        Task<Dictionary<string, int>> GetOrdreStatutCountsAsync(int agentId);
        Task<List<HistoriqueOrdre>> GetHistoriqueByOrdreIdAsync(int ordreId);
        Task<List<Ordre>> GetOrdresByStatutAsync(string statut);
    }
}
