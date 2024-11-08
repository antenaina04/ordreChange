using ordreChange.Repositories.Interfaces;

namespace ordreChange.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrdreRepository Ordres { get; }
        IAgentRepository Agents { get; }
        Task<int> CompleteAsync();
    }
}
