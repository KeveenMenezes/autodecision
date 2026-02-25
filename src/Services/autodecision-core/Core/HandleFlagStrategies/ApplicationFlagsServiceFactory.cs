using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class ApplicationFlagsServiceFactory : IApplicationFlagsServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationFlagsServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IApplicationFlagsService GetService() =>
            _serviceProvider.GetRequiredService<IApplicationFlagsService>();
    }
}