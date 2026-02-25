using AutodecisionCore.AutoApprovalCore.Rules;
using AutodecisionCore.Core.AutoApproval.Factory.Interface;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Factory
{
	public class DefaultItemFactory : IAutoApprovalItemFactory
	{
		public List<IAutoApprovalItem> CreateItems(AutoApprovalRequest request, IFeatureToggleClient featureToggleClient, ILogger<AutoApprovalManager> logger, ICreditRiskService creditRiskService)
		{
			return new List<IAutoApprovalItem>
			{
				new ApplicationParametersRule(featureToggleClient, logger, creditRiskService),
				new CommitmentLevelRule(featureToggleClient, logger),
				new EndBalanceCommitmentLevelRule(featureToggleClient, logger),
				new LengthOfEmploymentRule(featureToggleClient, logger),
				new OpenBankingOpenPayrollBankAccountRule(featureToggleClient, logger),
				new DebitCardIssuerRule(featureToggleClient, logger),
				new FaceRecognitionRule(featureToggleClient, logger),
				new FaceRecognitionOnexNRule(featureToggleClient, logger),
				new AllotmentRule(featureToggleClient, logger),
				new CreditPolicyRule(featureToggleClient, logger),
                new DueDiligenceRule(featureToggleClient, logger)
            };
		}
	}
}
