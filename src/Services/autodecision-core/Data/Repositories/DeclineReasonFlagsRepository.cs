using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;
using AutodecisionCore.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutodecisionCore.Data.Repositories
{
    public class DeclineReasonFlagsRepository : BaseRepository<DeclineReasonFlags>, IDeclineReasonFlagsRepository
    {
        public DeclineReasonFlagsRepository(DatabaseContext dbContext) : base(dbContext) { }

        public async Task<List<DeclineReasonFlags>> GetAllActiveAsync() => await _dbSet.Where(f => f.IsDeleted != true).ToListAsync();

    }
}
