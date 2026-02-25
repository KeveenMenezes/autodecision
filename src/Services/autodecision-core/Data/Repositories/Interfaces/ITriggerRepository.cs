using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Models.Trigger;
using AutodecisionCore.Data.Repositories.Base;

namespace AutodecisionCore.Data.Repositories.Interfaces
{
    public interface ITriggerRepository: IBaseRepository<Trigger>
    {
        Task<Trigger> FindByDescriptionAsync(string description);
        Task<Trigger> FindByDescriptionIncludeTriggerFlagsAsync(string description);
    }
}
