using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies.Interfaces
{
    public interface IApplicationFlagsBinder
    {
        Task BindApplicationFlagsAsync(Application application, ApplicationCore applicationCore);
    }
}
