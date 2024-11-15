using ordreChange.Models;

namespace ordreChange.Repositories.Interfaces
{
    public interface IOrdreRepository : IRepository<Ordre>
    {
        Task<List<HistoriqueOrdre>> GetHistoriqueByOrdreIdAsync(int ordreId);
        Task<List<Ordre>> GetOrdresByStatutAsync(string statut);
        Task<Dictionary<string, int>> GetStatutCountsAsync();
        Task<bool> ValiderOrdreAsync(int ordreId);
        Task<bool> UpdateStatutOrdreAsync(int ordreId, string statut);
        Task AjouterHistoriqueAsync(Ordre ordre, string action);
    }
}
