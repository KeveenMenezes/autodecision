using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;
using AutodecisionCore.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutodecisionCore.Data.Repositories
{
    public class FlagRepository : BaseRepository<Flag>, IFlagRepository
    {
        public FlagRepository(DatabaseContext dbContext) : base(dbContext) { }

        public async Task<List<Flag>> GetAllActiveFlagsAsync() => 
            await _dbSet.Where(f => f.Active).ToListAsync();

        public async Task<List<Flag>> GetAllActiveInternalFlagsAsync() =>
            await _dbSet.Where(f => f.Active && f.InternalFlag).ToListAsync();

        public async Task<Flag> GetFlagByCodeAsync(string code) =>
          await _dbSet.Where(x => x.Code == code).FirstOrDefaultAsync();

        public async Task<List<Flag>> GetAllFlagsAsync() =>
            await _dbSet.ToListAsync();

		public async Task<List<Flag>> GetAllActiveFlagsExceptWarningAsync() =>
	        await _dbSet.Where(f => f.Active && f.IsWarning == false).ToListAsync();

		public async Task<List<Flag>> GetAllActiveInternalFlagsExceptWarningAsync() =>
			await _dbSet.Where(f => f.Active && f.InternalFlag && f.IsWarning == false).ToListAsync();
	}
}
