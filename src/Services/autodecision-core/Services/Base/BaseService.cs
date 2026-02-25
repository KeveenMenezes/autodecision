using Microsoft.EntityFrameworkCore;
using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Data.Repositories.Base;
using AutodecisionCore.Extensions;
using AutodecisionCore.DTOs;

namespace AutodecisionCore.Services.Base
{
    public interface IBaseService<T>
        where T : BaseModel
    {
        IQueryable<T> QueryWithBaseFilters(BaseFilterDto filter);
        Task<List<T>> GetWithFilterAsync(BaseFilterDto filter, int? userId = null);
        Task<T> CreateAsync(T entity, int? userId = null);
        Task<T> UpdateAsync(T entity, int? userId = null);
        Task<bool> DeleteAsync(int id, int? userId = null, bool setIsDeleted = false);
    }

    public abstract class BaseService<T> : IBaseService<T>
        where T : BaseModel
    {
        private readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public IQueryable<T> QueryWithBaseFilters(BaseFilterDto filter)
        {
            var query = _repository.Query();

            if ((filter.Id ?? 0) > 0)
                query = query.Where(w => w.Id == filter.Id);

            if (filter.CreateAtStart.HasValue && filter.CreateAtEnd.HasValue)
                query = query.Where(
                    w =>
                        w.CreatedAt >= filter.CreateAtStart
                        && w.CreatedAt <= filter.CreateAtEnd.Value.AddDays(1)
                );
            else if (filter.CreateAtStart.HasValue)
                query = query.Where(w => w.CreatedAt >= filter.CreateAtStart);
            else if (filter.CreateAtEnd.HasValue)
                query = query.Where(w => w.CreatedAt <= filter.CreateAtEnd.Value.AddDays(1));

            if (filter.IsDeleted.HasValue)
                query = query.Where(w => w.IsDeleted == filter.IsDeleted);

            return query;
        }

        public virtual Task<List<T>> GetWithFilterAsync(BaseFilterDto filter, int? userId = null) =>
            QueryWithBaseFilters(filter).ToListAsync();

        public virtual Task<T> CreateAsync(T entity, int? userId = null) =>
            _repository.CreateAsync(entity);

        public virtual async Task<T> UpdateAsync(T entity, int? userId = null)
        {
            var notExists = !await _repository.ExistsAsync(e => e.Id == entity.Id);
            if (notExists)
            {
                Console.WriteLine($"{typeof(T)} id: {entity.Id} not found");
                throw new Exception(ExceptionMessages.NotFound);
            }

            return await _repository.UpdateAsync(entity);
        }

        public virtual async Task<bool> DeleteAsync(
            int id,
            int? userId = null,
            bool setIsDeleted = false
        )
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                Console.WriteLine($"{typeof(T)} id: {id} not found");
                throw new Exception(ExceptionMessages.NotFound);
            }

            if (setIsDeleted)
            {
                entity.IsDeleted = true;
                await _repository.UpdateAsync(entity);
                return true;
            }

            await _repository.RemoveAsync(r => r.Id == id);
            return true;
        }
    }
}
