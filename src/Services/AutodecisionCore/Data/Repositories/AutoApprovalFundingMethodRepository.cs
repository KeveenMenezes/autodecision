using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Confluent.Kafka.ConfigPropertyNames;

namespace AutodecisionCore.Data.Repositories
{

	public interface IAutoApprovalFundingMethodRepository : IBaseRepository<AutoApprovalFundingMethod>
	{
		Task<List<AutoApprovalFundingMethod>> GetAll();
	}

	public class AutoApprovalFundingMethodRepository : BaseRepository<AutoApprovalFundingMethod>, IAutoApprovalFundingMethodRepository
	{
		private readonly IMemoryCache _cache;

		public AutoApprovalFundingMethodRepository(DatabaseContext dbContext, IMemoryCache cache) : base(dbContext)
		{
			_cache = cache;
		}

		public async Task<List<AutoApprovalFundingMethod>> GetAll() 
		{
			var cacheKey = "AutoApprovalFundingMethods";
			List<AutoApprovalFundingMethod> autoApprovalFundingMethods;

			if (!_cache.TryGetValue(cacheKey, out autoApprovalFundingMethods))
			{
				autoApprovalFundingMethods = await _dbSet.Select(x => x).AsNoTracking().ToListAsync();

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromHours(1));

				_cache.Set(cacheKey, autoApprovalFundingMethods, cacheOptions);
			}

			return autoApprovalFundingMethods;
		}
	}
}
