using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class LengthOfEmploymentRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public LengthOfEmploymentRule(
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
                RuleName = "LengthOfEmploymentRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleLengthOfEmployment"))
                    return Task.CompletedTask;

                if (request.LastApplications.Any(x => x.Status == ApplicationStatus.Liquidated))
                {
                    ruleResult.Status = AutoApprovalResultEnum.Ignored;
                    ruleResult.Description = "Customer has a paid in full application.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (request.CreditPolicy == null)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Ignored;
                    ruleResult.Description = "Credit policy not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                int? minValue = 0;

                if (_featureToggleClient.IsEnabled("stop_old_credit_policy"))
                {
                    minValue = request.Application.Type == ApplicationType.NewLoan ? request.CreditPolicy.CreditPolicyEntity.LoeNew : request.CreditPolicy.CreditPolicyEntity.LoeRefi;

                    if (minValue == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Ignored;
                        ruleResult.Description = "Length of Employment rule not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }
                }
                else
                {
                    if (request.CreditPolicy.EmployerRules.EmployerRulesItems == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Ignored;
                        ruleResult.Description = "Employer rules not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    var rule = request.CreditPolicy.EmployerRules.EmployerRulesItems.Where(x => x.Key == "length_of_employment").FirstOrDefault();

                    if (rule == null)
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Ignored;
                        ruleResult.Description = "Length of Employment rule not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    // if rule exists parse it.
                    minValue = int.Parse(rule.Min);
                }

                if (!IsValidHireDate(request))
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = "The field Verified Date Of Hire cannot be null.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (!request.Application.VerifiedDateOfHire.HasValue)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = "Verified date of hire not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                var lenghtOfEmployment = (DateTime.Today - request.Application.VerifiedDateOfHire.Value).Days;

                if (lenghtOfEmployment < minValue)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Employment lenght cannot be less than {minValue} days.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: LengthOfEmploymentRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: LengthOfEmploymentRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
        }

        private static bool IsValidHireDate(AutoApprovalRequest request)
        {
            if (!request.Application.VerifiedDateOfHire.HasValue || request.Application.VerifiedDateOfHire == null)
                return false;

            return true;
        }
    }
}