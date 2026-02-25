using AutodecisionCore.Core.AutoApproval.Factory.Interface;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Factory
{
	public class CashlessItemFactory : IAutoApprovalItemFactory
	{
		public List<IAutoApprovalItem> CreateItems(AutoApprovalRequest request, IFeatureToggleClient featureToggleClient, ILogger<AutoApprovalManager> logger, ICreditRiskService creditRiskService)
		{
			return new List<IAutoApprovalItem> { new CashlessRule(featureToggleClient, logger) };
		}
	}
}
