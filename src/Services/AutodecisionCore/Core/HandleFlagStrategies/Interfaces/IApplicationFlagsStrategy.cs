using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies.Interfaces
{
    public interface IApplicationFlagsStrategy
    {
        /// <summary>
        /// Gets the priority level of the strategy.
        /// Higher priority values indicate greater importance.
        /// The default and highest priority level is 1.
        /// Strategies that lead to the final validation flow and that surpass any other strategy are level 1 by i.e.
        /// Business rules that do not exceed the previous rule would be level 2 on this scale.
        /// </summary>
        int Priority => 1;
        bool CanHandle(Application application, ApplicationCore applicationCore);
        Task BindFlagsAsync(Application application, ApplicationCore applicationCore);
    }
}