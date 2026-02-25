using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class RefiFlagsStrategy : IApplicationFlagsStrategy
    {
        private readonly IApplicationFlagsServiceFactory _applicationFlagsServiceFactory;

        public RefiFlagsStrategy(IApplicationFlagsServiceFactory applicationFlagsServiceFactory)
        {
            _applicationFlagsServiceFactory = applicationFlagsServiceFactory;
        }

        public int Priority => 2;

        public async Task BindFlagsAsync(Application application, ApplicationCore applicationCore)
        {
            var _applicationFlagsService = _applicationFlagsServiceFactory.GetService();
            //Temporary removal until further understanding.
            //await _applicationFlagsService.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.EmploymentLength, description: "Flag Ignored due to Refi Strategy");
        }

        //public bool CanHandle(Application application, ApplicationCore applicationCore) =>
        //application.Type == ApplicationType.Refi;

        public bool CanHandle(Application application, ApplicationCore applicationCore) 
        {
            return false;
        }
    }
}