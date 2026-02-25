using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Core.HandleFlagStrategies
{
    public class CashbackFlagsStrategy : IApplicationFlagsStrategy
    {
        private readonly IApplicationFlagsServiceFactory _applicationFlagsServiceFactory;

        public CashbackFlagsStrategy(IApplicationFlagsServiceFactory applicationFlagsServiceFactory)
        {
            _applicationFlagsServiceFactory = applicationFlagsServiceFactory;
        }

        public int Priority => 3;

        public async Task BindFlagsAsync(Application application, ApplicationCore applicationCore)
        {
            var _applicationFlagsService = _applicationFlagsServiceFactory.GetService();
            await _applicationFlagsService.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.EmploymentLength, description: "Flag Ignored due to Cashback Strategy");
        }

        public bool CanHandle(Application application, ApplicationCore applicationCore)
        {
            List<int> cashbackProductIds = new() { ApplicationProductId.Cashback, ApplicationProductId.LowIncomeCashback };
            return cashbackProductIds.Contains(application.ProductId);
        }
    }
}