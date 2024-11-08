using Microsoft.EntityFrameworkCore;
using ordreChange.Data;
using ordreChange.Repositories.Interfaces;
using System.Linq.Expressions;

namespace ordreChange.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly OrdreDeChangeContext _context;
        private readonly DbSet<T> _entities;

        public Repository(OrdreDeChangeContext context)
        {
            _context = context;
            _entities = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _entities.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _entities.ToListAsync();

        public async Task AddAsync(T entity) => await _entities.AddAsync(entity);

        public void Update(T entity) => _entities.Update(entity);

        public void Remove(T entity) => _entities.Remove(entity);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }
    }
}
