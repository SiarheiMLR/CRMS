using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CRMS.Domain.Interfaces;

namespace CRMS.TestData
{
    public class TestRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly List<TEntity> _data;

        public TestRepository(List<TEntity> initialData = null)
        {
            _data = initialData ?? new List<TEntity>();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return Task.FromResult(_data.AsEnumerable());
        }

        public Task<TEntity> GetByIdAsync(int id)
        {
            var entity = _data.FirstOrDefault(e => e.GetType().GetProperty("Id")?.GetValue(e) is int entityId && entityId == id);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.FromResult(_data.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(TEntity entity)
        {
            _data.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(TEntity entity)
        {
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(entity);
                var existingEntity = _data.FirstOrDefault(e => idProperty.GetValue(e)?.Equals(idValue) == true);
                if (existingEntity != null)
                {
                    var index = _data.IndexOf(existingEntity);
                    _data[index] = entity;
                }
            }
        }

        public void Remove(TEntity entity)
        {
            _data.Remove(entity);
        }
    }
}
