using Bookify.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Interfaces
{
    public interface IRepositry<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T>? GetById(int id);
        Task AddAsync(T entity);
        void Remove(T entity);
        void Update(T entity);

    }
    public class Repository<T>: IRepositry<T> where T : class
    {
        private readonly AppDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public Repository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
           await _dbSet.AddAsync(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T>? GetById(int id)
        {
            return await _dbSet.FindAsync(id) ;
        }

        public void Remove(T entity)
        {
             _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
