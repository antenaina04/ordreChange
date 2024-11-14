using Microsoft.EntityFrameworkCore;
using ordreChange.Data;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;

namespace ordreChange.Repositories.Implementations
{
    public class OrdreRepository : Repository<Ordre>, IOrdreRepository
    {
        public OrdreRepository(OrdreDeChangeContext context) : base(context)
        {
        }
        public async Task<List<Ordre>> GetAllAsync()
        {
            return await _context.Ordres.ToListAsync();
        }
        public async Task<List<HistoriqueOrdre>> GetHistoriqueByOrdreIdAsync(int ordreId)
        {
            return await _context.HistoriqueOrdres
                .Where(h => h.IdOrdre == ordreId)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }
        
    }
}
