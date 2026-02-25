using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class ApplicationFlagsBinder : IApplicationFlagsBinder
    {
        private readonly List<IApplicationFlagsStrategy> _strategies;
        private readonly IDefaultFlagsStrategy _defaultStrategy;

        public ApplicationFlagsBinder(
            IEnumerable<IApplicationFlagsStrategy> strategies,
            IDefaultFlagsStrategy defaultStrategy)
        {
            _strategies = strategies.ToList();
            _defaultStrategy = defaultStrategy;
        }

        public async Task BindApplicationFlagsAsync(Application application, ApplicationCore applicationCore)
        {
            var compatibleStrategies = _strategies.Where(x => x.CanHandle(application, applicationCore)).ToList();
            if (!compatibleStrategies.Any())
            {
                await _defaultStrategy.BindFlagsAsync(application, applicationCore);
                return;
            }
            var highestPriorityStrategy = compatibleStrategies.OrderBy(x => x.Priority).First();
            await highestPriorityStrategy.BindFlagsAsync(application, applicationCore);
        }
    }
}