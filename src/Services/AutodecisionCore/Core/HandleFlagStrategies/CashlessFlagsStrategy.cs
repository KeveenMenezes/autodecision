using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class CashlessFlagsStrategy : IApplicationFlagsStrategy
    {
        private readonly IApplicationFlagsServiceFactory _applicationFlagsServiceFactory;

        public CashlessFlagsStrategy(IApplicationFlagsServiceFactory applicationFlagsServiceFactory)
        {
            _applicationFlagsServiceFactory = applicationFlagsServiceFactory;
        }

        public bool CanHandle(Application application, ApplicationCore applicationCore) =>
            application.ProductId == ApplicationProductId.Cashless;

        public async Task BindFlagsAsync(Application application, ApplicationCore applicationCore)
        {
            var _applicationFlagsService = _applicationFlagsServiceFactory.GetService();

            if (application.PaymentType == PayrollType.DebitCard)
                await _applicationFlagsService.AddOnlyInternalApplicationFlagsToRegister(applicationCore);
            else
                await _applicationFlagsService.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.CustomerIdentityFlag, description: "Flag Ignored due to Cashless Strategy");
        }
    }
}