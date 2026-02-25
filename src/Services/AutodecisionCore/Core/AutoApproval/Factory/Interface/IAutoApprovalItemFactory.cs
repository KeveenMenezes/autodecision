using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Factory.Interface
{
	public interface IAutoApprovalItemFactory
	{
		List<IAutoApprovalItem> CreateItems(AutoApprovalRequest request, IFeatureToggleClient featureToggleClient, ILogger<AutoApprovalManager> logger, ICreditRiskService creditRiskService);
	}
}
