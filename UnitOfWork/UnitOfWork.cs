﻿using ordreChange.Data;
using ordreChange.Models;
using ordreChange.Repositories.Implementations;
using ordreChange.Repositories.Interfaces;

namespace ordreChange.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrdreDeChangeContext _context;
        public IOrdreRepository Ordres { get; private set; }
        public IAgentRepository Agents { get; private set; }
        public IRepository<HistoriqueOrdre> HistoriqueOrdres { get; }

        public UnitOfWork(OrdreDeChangeContext context)
        {
            _context = context;
            Ordres = new OrdreRepository(_context);
            Agents = new AgentRepository(_context);
            HistoriqueOrdres = new Repository<HistoriqueOrdre>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
