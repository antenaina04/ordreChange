using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Services.Interfaces.IRoleServices
{
    public interface IValidateurService : IBaseRoleService
    {
        Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
        Task<Dictionary<string, int>> GetOrdreStatutCountsAsync();
        Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(string statut);
    }
}
