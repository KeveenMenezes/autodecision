using AutodecisionCore.Core.AutoApproval.Factory.Interface;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Core.AutoApprovalCore;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using AutodecisionCore.AutoApprovalCore.Rules;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Core.AutoApproval.Factory
{
    public class RequiredItemFactory : IAutoApprovalItemFactory
    {
        public List<IAutoApprovalItem> CreateItems(AutoApprovalRequest request, IFeatureToggleClient featureToggleClient, ILogger<AutoApprovalManager> logger, ICreditRiskService creditRiskService)
        {
            return new List<IAutoApprovalItem>
            {
                new CommitmentLevelRule(featureToggleClient, logger),
                new LengthOfEmploymentRule(featureToggleClient, logger),
                new ApplicationParametersRule(featureToggleClient, logger, creditRiskService),
                new DebitCardIssuerRule(featureToggleClient, logger),
                new DueDiligenceRule(featureToggleClient, logger)
            };
        }
    }
}