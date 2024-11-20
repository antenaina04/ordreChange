using ordreChange.Models;

namespace ordreChange.Services.Interfaces.IRoleServices
{
    public interface IValidateurService : IBaseRoleService
    {
        Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
        Task<Dictionary<string, int>> GetOrdreStatutCountsAsync();
        Task<List<Ordre>> GetOrdresByStatutAsync(string statut);
    }
}
