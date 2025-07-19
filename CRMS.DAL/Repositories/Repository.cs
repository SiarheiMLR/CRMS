using CRMS.DAL.Data;
using CRMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRMS.DAL.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly CRMSDbContext _context;
        protected readonly DbSet<TEntity> _entities;

        public Repository(CRMSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entities = context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(params string[] includeProperties)
        {
            IQueryable<TEntity> query = _entities;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetWhereNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AsNoTracking().Where(predicate).ToListAsync();
        }


        public async Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _entities.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _entities.Update(entity);
        }

        public void Remove(TEntity entity)
        {
            _entities.Remove(entity);
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            Update(entity); // обычный метод обновления
            return Task.CompletedTask;
        }
    }
}
