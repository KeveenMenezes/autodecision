using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutodecisionCore.Data.Repositories
{
	public interface IAutoApprovalPaymentTypeRepository : IBaseRepository<AutoApprovalPaymentType>
	{
		Task<List<AutoApprovalPaymentType>> GetAll();
	}

	public class AutoApprovalPaymentTypeRepository : BaseRepository<AutoApprovalPaymentType>, IAutoApprovalPaymentTypeRepository
	{
		private readonly IMemoryCache _cache;

		public AutoApprovalPaymentTypeRepository(DatabaseContext dbContext, IMemoryCache cache) : base(dbContext)
		{
			_cache = cache;
		}

		public async Task<List<AutoApprovalPaymentType>> GetAll() 
		{
			var cacheKey = "autoApprovalPaymentTypes";
			List<AutoApprovalPaymentType> autoApprovalPaymentTypes;

			if (!_cache.TryGetValue(cacheKey, out autoApprovalPaymentTypes))
			{
				autoApprovalPaymentTypes = await _dbSet.Select(x => x).AsNoTracking().ToListAsync();

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromHours(1));

				_cache.Set(cacheKey, autoApprovalPaymentTypes, cacheOptions);
			}

			return autoApprovalPaymentTypes;
		}
	}
}
