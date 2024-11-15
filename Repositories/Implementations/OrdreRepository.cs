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
        public async Task<List<Ordre>> GetOrdresByStatutAsync(string statut)
        {
            return await _context.Ordres
                .Where(o => o.Statut == statut)
                .Include(o => o.Agent)
                .ToListAsync();
        }
        public async Task<Dictionary<string, int>> GetStatutCountsAsync()
        {
            return await _context.Ordres
                .GroupBy(o => o.Statut)
                .Select(g => new { Statut = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Statut, g => g.Count);
        }
        public async Task<bool> ValiderOrdreAsync(int ordreId)
        {
            var ordre = await _context.Ordres.FindAsync(ordreId);
            if (ordre == null || ordre.Statut != "En attente")
                return false;

            ordre.Statut = "Validé";
            ordre.DateDerniereModification = DateTime.UtcNow;

            _context.Ordres.Update(ordre);
            return true;
        }
        public async Task<bool> UpdateStatutOrdreAsync(int ordreId, string statut)
        {
            var ordre = await _context.Ordres.FindAsync(ordreId);
            if (ordre == null || ordre.Statut != "En attente")
                return false;

            ordre.Statut = statut;
            ordre.DateDerniereModification = DateTime.UtcNow;

            _context.Ordres.Update(ordre);
            return true;
        }
        public async Task AjouterHistoriqueAsync(Ordre ordre, string action)
        {
            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = ordre.Statut,
                Action = action,
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };
            await _context.HistoriqueOrdres.AddAsync(historique);
        }
    }
}
