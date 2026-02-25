using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.AutoApprovalCore.Rules
{
    public class ApplicationParametersRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;
        private readonly ICreditRiskService _creditRiskService;

        public ApplicationParametersRule(
            IFeatureToggleClient featureToggleClient,
            ILogger<AutoApprovalManager> logger,
            ICreditRiskService creditRiskService)
        {
            _featureToggleClient = featureToggleClient;
            _logger = logger;
            _creditRiskService = creditRiskService;
        }

        public Task RunRule(AutoApprovalRequest request, AutoApprovalResponse response, ApplicationCore applicationCore)
        {
            var ruleResult = new AutoApprovalRule()
            {
                RuleName = "ApplicationParametersRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleApplicationParameters"))
                    return Task.CompletedTask;

                if (request.Application.EmployerId == EmployersIds.BrowardCountyFL)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Employment verification required.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (_creditRiskService.ValidateAllowedLoanToCalculateScore(request.Application, request.Employer))
                {
                    if (request.CreditRisk?.ApplicationScore == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = $"BMG Money Risk Score could not be calculated.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    if (!_creditRiskService.ValidateDocAllotmentSDD(request.Application, request.Employer, request.ApplicationDocuments))
                    {
                        var applicationScore = request.CreditRisk.ApplicationScore;
                        if (applicationScore.Score <= applicationScore.CutOff)
                        {
                            ruleResult.Status = AutoApprovalResultEnum.Pending;
                            ruleResult.Description = $"BMG Money Risk Scores lower then {applicationScore.CutOff} are not allowed in " +
                                $"Auto Approval (v{applicationScore.ModelVersion}). This application got {applicationScore.Score} (Group {applicationScore.Group}).";
                            response.AutoApprovalRules.Add(ruleResult);
                            return Task.CompletedTask;
                        }
                    }
                }

                var fundingMethodConfiguration = request.AutoApprovalFundingMethods.FirstOrDefault(x => x.FundingMethod == request.Application.FundingMethod);

                if (!fundingMethodConfiguration?.IsAllowed ?? false)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Funding method {request.Application.FundingMethod} is not allowed in Auto Approval.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                var paymentTypeConfiguration = request.AutoApprovalPaymentTypes.FirstOrDefault(x => x.PaymentType == request.Application.PaymentType);

                if (!paymentTypeConfiguration?.IsAllowed ?? false && request.Reason != Reason.AllFlagsApproved)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Payment type {request.Application.PaymentType} is not allowed in Auto Approval.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: ApplicationParametersRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: ApplicationParametersRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
        }
    }
}
