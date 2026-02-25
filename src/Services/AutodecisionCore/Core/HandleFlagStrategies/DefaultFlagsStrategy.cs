using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class DefaultFlagsStrategy : IDefaultFlagsStrategy
    {
        private readonly IApplicationFlagsServiceFactory _applicationFlagsServiceFactory;

        public DefaultFlagsStrategy(IApplicationFlagsServiceFactory applicationFlagsServiceFactory)
        {
            _applicationFlagsServiceFactory = applicationFlagsServiceFactory;
        }

        public async Task BindFlagsAsync(Application application, ApplicationCore applicationCore)
        {
            var _applicationFlagsService = _applicationFlagsServiceFactory.GetService();
            await _applicationFlagsService.AddApplicationFlagsToRegister(applicationCore);
        }
    }
}