using Microsoft.EntityFrameworkCore;
using AutodecisionCore.Data.Context;
using System.Linq.Expressions;

namespace AutodecisionCore.Data.Repositories.Base
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<List<T>> GetAllWithFiltersAsync(Expression<Func<T, bool>> predicate);
        T GetById(int id);
        Task<T> GetByIdAsync(int id);
        T Create(T obj);
        Task<T> CreateAsync(T obj);
        T Update(T obj);
        Task<T> UpdateAsync(T obj);
        void Remove(Expression<Func<T, bool>> predicate);
        Task RemoveAsync(Expression<Func<T, bool>> predicate);
        bool Exists(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        int Count(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        List<T> CreateList(List<T> list);
        Task<List<T>> CreateListAsync(List<T> list);
        List<T> UpdateList(List<T> list);
        Task<List<T>> UpdateListAsync(List<T> list);

        Task SaveChanges();
    }

    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected DatabaseContext _dbContext;
        protected DbSet<T> _dbSet;

        public BaseRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public IQueryable<T> Query() => _dbSet;

        public Task<List<T>> GetAllWithFiltersAsync(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).ToListAsync();

        public T GetById(int id) => _dbSet.Find(id);

        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public T Create(T obj)
        {
            var createdEntity = _dbSet.Add(obj).Entity;
            _dbContext.SaveChanges();
            return createdEntity;
        }

        public async Task<T> CreateAsync(T obj)
        {
            var createdEntity = _dbSet.Add(obj).Entity;
            await _dbContext.SaveChangesAsync();
            return createdEntity;
        }

        public T Update(T obj)
        {
            var updatedEntity = _dbSet.Update(obj).Entity;
            _dbContext.SaveChanges();
            return updatedEntity;
        }

        public async Task<T> UpdateAsync(T obj)
        {
            var updatedEntity = _dbSet.Update(obj).Entity;
            await _dbContext.SaveChangesAsync();
            return updatedEntity;
        }

        public void Remove(Expression<Func<T, bool>> predicate)
        {
            _dbSet.Where(predicate).ToList().ForEach(entity => _dbSet.Remove(entity));
            _dbContext.SaveChanges();
        }

        public async Task RemoveAsync(Expression<Func<T, bool>> predicate)
        {
            var list = await _dbSet.Where(predicate).ToListAsync();
            list.ForEach(entity => _dbSet.Remove(entity));
            _dbContext.SaveChanges();
        }

        public bool Exists(Expression<Func<T, bool>> predicate) => _dbSet.Any(predicate);

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => _dbSet.AnyAsync(predicate);

        public int Count(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).Count();

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate) => _dbSet.Where(predicate).CountAsync();

        public List<T> CreateList(List<T> list)
        {
            _dbSet.AddRange(list);
            _dbContext.SaveChanges();
            return list;
        }

        public async Task<List<T>> CreateListAsync(List<T> list)
        {
            await _dbSet.AddRangeAsync(list);
            await _dbContext.SaveChangesAsync();
            return list;
        }

        public List<T> UpdateList(List<T> list)
        {
            _dbSet.UpdateRange(list);
            _dbContext.SaveChanges();
            return list;
        }

        public async Task<List<T>> UpdateListAsync(List<T> list)
        {
            _dbSet.UpdateRange(list);
            await _dbContext.SaveChangesAsync();
            return list;
        }

        public async Task SaveChanges()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
