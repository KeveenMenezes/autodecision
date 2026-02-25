using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutodecisionCore.Data.Repositories
{
	public interface IAutoApprovalUwClusterRepository : IBaseRepository<AutoApprovalUwCluster>
	{
		Task<List<AutoApprovalUwCluster>> GetAll();
	}

	public class AutoApprovalUwClusterRepository : BaseRepository<AutoApprovalUwCluster>, IAutoApprovalUwClusterRepository
	{
		private readonly IMemoryCache _cache;

		public AutoApprovalUwClusterRepository(DatabaseContext dbContext, IMemoryCache cache) : base(dbContext)
		{
			_cache = cache;
		}

		public async Task<List<AutoApprovalUwCluster>> GetAll() 
		{
			var cacheKey = "autoApprovalUwClusters";
			List<AutoApprovalUwCluster> autoApprovalUwClusters;

			if (!_cache.TryGetValue(cacheKey, out autoApprovalUwClusters))
			{
				autoApprovalUwClusters = await _dbSet.Select(x => x).AsNoTracking().ToListAsync();

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromHours(1));

				_cache.Set(cacheKey, autoApprovalUwClusters, cacheOptions);
			}

			return autoApprovalUwClusters;
		} 
	}
}
