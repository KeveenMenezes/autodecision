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
    public class DebitCardIssuerRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public DebitCardIssuerRule(
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
                RuleName = "DebitCardIssuerRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleDebitCardIssuer"))
                    return Task.CompletedTask;

                if (request.Application.PaymentType != PayrollType.DebitCard && request.Application.FundingMethod != ApplicationFundingMethod.DebitCard)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Ignored;
                    ruleResult.Description = "Payment type or funding method must be debit card.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (!request.DebitCard.IsConnected)
                {
                    SetAutoApprovalResult(ruleResult, response, AutoApprovalResultEnum.Pending, "Debit Card not connected / The issuer of Debit Card must be the same of the customer's bank account.");
                    return Task.CompletedTask;
                }

                if (request.Application.IsWebBankRollout && request.DebitCard.Vendor.ToLower() != DebitCardVendor.Tabapay)
                {
                    SetAutoApprovalResult(ruleResult, response, AutoApprovalResultEnum.Pending, "Customer's active debit card connection does not match rollout definition.");
                    return Task.CompletedTask;
                }

                if (!request.Application.IsWebBankRollout && request.DebitCard.Vendor.ToLower() != DebitCardVendor.PayNearMe)
                {
                    var message = "Customer's active debit card connection doest not match Paynearme";
                    SetAutoApprovalResult(ruleResult, response, AutoApprovalResultEnum.Pending, message);
                    return Task.CompletedTask;
                }

                SetAutoApprovalResult(ruleResult, response, AutoApprovalResultEnum.Approved);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: DebitCardIssuerRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: DebitCardIssuerRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
        }

        private static void SetAutoApprovalResult(AutoApprovalRule ruleResult, AutoApprovalResponse response, AutoApprovalResultEnum status, string description = "")
        {
            ruleResult.Status = status;
            ruleResult.Description = description;
            response.AutoApprovalRules.Add(ruleResult);
        }
    }
}
