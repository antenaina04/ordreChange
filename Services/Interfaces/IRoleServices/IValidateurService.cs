using ordreChange.Models;
using OrdreChange.Dtos;

namespace ordreChange.Services.Interfaces.IRoleServices
{
    public interface IValidateurService : IBaseRoleService
    {
        Task<Dictionary<string, int>> GetOrdreStatutCountsAsync();
        Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(string statut);
    }
}
