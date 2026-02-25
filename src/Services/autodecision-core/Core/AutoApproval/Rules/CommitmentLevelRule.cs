using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class CommitmentLevelRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public CommitmentLevelRule(
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
                RuleName = "CommitmentLevelRule",
            };


            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleCommitmentLevel"))
                    return Task.CompletedTask;

                if (!AreTheAmountOfPaymentAndNetIncomeValid(request))
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = "The fields VerifiedNetIncome, AmountOfPayment and MaxValue cannot be null.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (!request.Application.VerifiedNetIncome.HasValue || request.Application.VerifiedNetIncome.Value == decimal.Zero)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = "The field VerifiedNetIncome cannot be zero.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                decimal? maxValue = 0;

                if (_featureToggleClient.IsEnabled("stop_old_credit_policy"))
                {
                    maxValue = request.Application.Type == ApplicationType.NewLoan ? request.CreditPolicy.CreditPolicyEntity.LocNew : request.CreditPolicy.CreditPolicyEntity.LocRefi;

                    if (maxValue == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = "Commitment level rule not defined.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    if (request.CreditPolicy.EmployerRules.EmployerRulesItems == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = "Employer rules not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    var rule = request.CreditPolicy.EmployerRules.EmployerRulesItems.Where(x => x.Key == "commitment_level").FirstOrDefault();

                    if (rule == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = "Commitment level rule not defined.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    decimal locLevel = 0;

                    if (!Decimal.TryParse(rule.Max, out locLevel))
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = "Commitment level rule was defined, but without parameters.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    // if rule exists parse it
                    maxValue = locLevel;
                }

				decimal additionalIncome = request.Application.AdditionalIncome.HasValue ? request.Application.AdditionalIncome.Value : 0;
				var commitmentLevel = request.Application.AmountOfPayment / (request.Application.VerifiedNetIncome.Value + additionalIncome);

                if (Math.Round(commitmentLevel, 4) > Math.Round(maxValue ?? 0, 4))
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"The customer's commitment level {(commitmentLevel * 100).ToString("N2")}% exceeds the maximum value allowed {(maxValue * 100)?.ToString("N0")}%";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: CommitmentLevelRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: CommitmentLevelRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }

        }

        private static bool AreTheAmountOfPaymentAndNetIncomeValid(AutoApprovalRequest request)
        {
            if (!request.Application.VerifiedNetIncome.HasValue || request.Application.VerifiedNetIncome == null)
                return false;

            if (request.Application.AmountOfPayment == decimal.Zero)
                return false;

            return true;
        }
    }
}
