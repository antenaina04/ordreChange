using ordreChange.Models;
using ordreChange.Repositories.Interfaces;

namespace ordreChange.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrdreRepository Ordres { get; }
        IAgentRepository Agents { get; }
        IRepository<HistoriqueOrdre> HistoriqueOrdres { get; }
        Task<int> CompleteAsync();
    }
}
