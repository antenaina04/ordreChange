using ordreChange.Controllers;
using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Services.Interfaces
{
    public interface IOrdreService
    {
        Task<OrdreResponseDto> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible);
        Task<OrdreDto?> GetOrdreDtoByIdAsync(int id);
        //Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
        Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string statut);
        Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto ordreModifications);
        Task<object> GetOrdreStatutCountsAsync(int agentId);
        Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(int agentId, string statut);
        Task<HistoriqueDto?> GetHistoriqueDtoByOrdreIdAsync(int agentId, int ordreId);
    }
}
