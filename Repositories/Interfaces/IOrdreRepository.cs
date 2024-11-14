using ordreChange.Models;

namespace ordreChange.Repositories.Interfaces
{
    public interface IOrdreRepository : IRepository<Ordre>
    {
        Task<List<HistoriqueOrdre>> GetHistoriqueByOrdreIdAsync(int ordreId);
        Task<List<Ordre>> GetOrdresByStatutAsync(string statut);

    }
}
