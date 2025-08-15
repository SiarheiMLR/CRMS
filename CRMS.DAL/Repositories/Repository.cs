using CRMS.DAL.Data;
using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public async Task<TEntity?> GetByIdAsync(int id, params string[] includes)
        {
            IQueryable<TEntity> query = _entities;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // Предполагается, что TEntity имеет свойство Id
            return await query.FirstOrDefaultAsync(e =>
                EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<TEntity>> GetWhereNoTrackingAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AsNoTracking().Where(predicate).ToListAsync();
        }

        // Новый метод для получения IQueryable
        public IQueryable<TEntity> AsQueryable()
        {
            return _entities.AsQueryable();
        }

        // Новый метод для загрузки с включением связанных данных
        public async Task<IEnumerable<TEntity>> FindWithIncludesAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null)
        {
            IQueryable<TEntity> query = _entities;

            if (includes != null)
            {
                query = includes(query);
            }

            return await query
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
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

        public async Task<IEnumerable<Queue>> GetQueuesWithSlaAsync()
        {
            return await _context.Queues.ToListAsync();
        }

        public async Task<Queue> GetByEmailAsync(string address)
        {
            return await _context.Queues
                .FirstOrDefaultAsync(q => q.CorrespondAddress == address || q.CommentAddress == address);
        }

        // Метод возвращает очереди, разрешенные для списка groupId
        public async Task<List<Queue>> GetQueuesForGroupIdsAsync(IEnumerable<int> groupIds)
        {
            if (!groupIds?.Any() ?? true)
                return new List<Queue>();

            var ids = groupIds.Where(id => id > 0).Distinct().ToList();
            if (ids.Count == 0) return new List<Queue>();

            // Используем ручное построение OR-условий
            var query = _context.Set<QueuePermission>().AsQueryable();
            var predicate = PredicateBuilder.New<QueuePermission>(false);

            foreach (var id in ids)
            {
                int tempId = id; // Для избежания замыкания
                predicate = predicate.Or(p => p.GroupId == tempId);
            }

            return await query
                .Where(predicate)
                .Select(p => p.Queue)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }
    }
}