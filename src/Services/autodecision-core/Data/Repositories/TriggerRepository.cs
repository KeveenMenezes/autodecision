using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models.Trigger;
using AutodecisionCore.Data.Repositories.Base;
using AutodecisionCore.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8603

namespace AutodecisionCore.Data.Repositories
{
    public class TriggerRepository : BaseRepository<Trigger>, ITriggerRepository
    {
        public TriggerRepository(DatabaseContext dbContext) : base(dbContext) { }

        public async Task<Trigger> FindByDescriptionAsync(string description) =>
          await _dbSet
            .Where(t => t.Description == description)
            .FirstOrDefaultAsync();

        public async Task<Trigger> FindByDescriptionIncludeTriggerFlagsAsync(string description) =>
             await _dbSet
            .Include(t => t.Flags)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Description == description);
    }
}
