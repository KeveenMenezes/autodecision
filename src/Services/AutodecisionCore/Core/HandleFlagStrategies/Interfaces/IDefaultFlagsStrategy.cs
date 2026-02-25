using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies.Interfaces
{
    public interface IDefaultFlagsStrategy
    {
        Task BindFlagsAsync(Application application, ApplicationCore applicationCore);
    }
}
