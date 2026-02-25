using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class DueDiligenceRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public DueDiligenceRule(
            IFeatureToggleClient featureToggleClient,
            ILogger<AutoApprovalManager> logger)
        {
            _featureToggleClient = featureToggleClient;
            _logger = logger;
        }

        public Task RunRule(AutoApprovalRequest request, AutoApprovalResponse response, ApplicationCore applicationCore)
        {
            var ruleResult = new AutoApprovalRule()
            {
                RuleName = "DueDiligenceRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleDueDiligence"))
                    return Task.CompletedTask;

                if (request.Employer.DueDiligenceStatus == (int)DueDiligenceStatusEnum.Rejected
                    || request.Employer.DueDiligenceStatus == (int)DueDiligenceStatusEnum.UnderReview)
                {
                    AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, DueDiligenceRuleConstants.DueDiligenceRejected, AutoApprovalResultEnum.Pending);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: DueDiligenceRule | Error: {ex.Message}");

                AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, $"Rule Name: DueDiligenceRule | Error: {ex.Message} ", AutoApprovalResultEnum.Error);
                return Task.CompletedTask;
            }
        }
    }
}
