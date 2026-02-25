using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.AutoApprovalCore.Interface;
using AutodecisionCore.Core.AutoApproval.Factory.Interface;
using AutodecisionCore.Core.AutoApproval.Factory;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Core.AutoApprovalCore
{
    public class AutoApprovalManager : IAutoApprovalManager
    {
		private readonly ILogger<AutoApprovalManager> _logger;
		private readonly IFeatureToggleClient _featureToggleClient;
		private readonly ICreditRiskService _creditRiskService;

		public AutoApprovalManager(
            ILogger<AutoApprovalManager> logger,
			IFeatureToggleClient featureToggleClient,
			ICreditRiskService creditRiskService
			)
        {
            _logger = logger;
			_featureToggleClient = featureToggleClient;
			_creditRiskService = creditRiskService;

		}

        public async Task RunRule(AutoApprovalRequest request, ApplicationCore applicationCore)
        {
			_logger.LogInformation($"Auto Approval started. LoanNumber: {request.LoanNumber}");

			List<IAutoApprovalItem> items = new();

			AutoApprovalItemFactory factory = new AutoApprovalItemFactory();
			IAutoApprovalItemFactory itemFactory = factory.GetFactory(request, applicationCore);

			items.AddRange(itemFactory.CreateItems(request, _featureToggleClient, _logger, _creditRiskService));

			var response = new AutoApprovalResponse()
			{
				AutoApprovalRules = new List<AutoApprovalRule>()
			};

            foreach (var item in items)
            {
                await item.RunRule(request, response, applicationCore);
            }

			applicationCore.ClearAutoApprovalRules();

			foreach (var item in response.AutoApprovalRules)
			{
				applicationCore.AddAutoApprovalRule(item);
			}

			applicationCore.Validate();

			_logger.LogInformation($"Auto Approval finished. LoanNumber: {request.LoanNumber}");			
        }
    }
}
