using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using System.Text.RegularExpressions;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class AllotmentRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public AllotmentRule(
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
                RuleName = "AllotmentRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleAllotment"))
                    return Task.CompletedTask;

                if (response.AutoApprovalRules.Any(x => x.Status == AutoApprovalResultEnum.Pending || x.Status == AutoApprovalResultEnum.Denied))
                    return Task.CompletedTask;

                if (request.Application.PaymentType != "allotment" && request.Application.PaymentType != "split_direct_deposit")
                {
                    AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.PaymentTypeNotAllotment, AutoApprovalResultEnum.Ignored);
                    return Task.CompletedTask;
                }

                var payAllocations = request.OpenPayroll?.Connections?.SelectMany(x => x.PayAllocations);
                PayAllocations? allotmentSddFound = FindAlltomentSdd(request, payAllocations);

                var bmgAllotments = request.OpenPayroll?.Connections?.SelectMany(x => x.BmgAllotments);
                BmgAllotments? BmgAllotmentsFound = FindBmgAllotments(request, bmgAllotments);

                if (allotmentSddFound != null || BmgAllotmentsFound != null)
                {
                    if (applicationCore.HasPendingAllotmentFlag())
                        applicationCore.ProcessFlag(FlagCode.AllotmentValidation, "Processed by AutoApproval AllotmentRule");

                    AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.AllotmentFoundNeeded, AutoApprovalResultEnum.Approved);
                    return Task.CompletedTask;
                }

                var lastBookedApplication = request.LastApplications
                    .Where(x => x.Status == ApplicationStatus.Booked)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                if (lastBookedApplication == null)
                {
                    CheckAllotmentNecessity(request, response, ruleResult);
                    return Task.CompletedTask;
                }

                var reconciliationSystemChanged = lastBookedApplication.ReconciliationSystem != request.Application.ReconciliationSystem;
                var valueChanged = lastBookedApplication.AmountOfPayment != request.Application.AmountOfPayment;

                if (reconciliationSystemChanged || valueChanged)
                {
                    AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.ReconciliationChanged, AutoApprovalResultEnum.PendingAllotment);
                    return Task.CompletedTask;
                }

                if (request.Application.Program != lastBookedApplication.Program)
                {
                    AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.ProgramChanged, AutoApprovalResultEnum.PendingAllotment);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: AllotmentRule | Error: {ex.Message}");

                AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, $"Rule Name: AllotmentRule | Error: {ex.Message} ", AutoApprovalResultEnum.Error);
                return Task.CompletedTask;
            }
        }

        private static PayAllocations? FindAlltomentSdd(AutoApprovalRequest request, IEnumerable<PayAllocations> payAllocations)
        {

            if (payAllocations == null)
                return null;

            if (request.Application.AllotmentAccountNumber is "000" or null)
                return null;

            foreach (var payAllocation in payAllocations)
            {
                if (payAllocation.Value != request.Application.AmountOfPayment)
                    continue;

                var accountNumber = Regex.Match(payAllocation.AccountNumber, @"\d+").Value;
                var routingNumber = Regex.Match(payAllocation.RoutingNumber, @"\d+").Value;

                if (request.Application.AllotmentRoutingNumber.EndsWith(routingNumber)
                    && request.Application.AllotmentAccountNumber.EndsWith(accountNumber))
                {
                    return payAllocation;
                }
            }

            return null;
        }

        private static BmgAllotments? FindBmgAllotments(AutoApprovalRequest request, IEnumerable<BmgAllotments> bmgAllotments)
        {

            if (bmgAllotments == null)
                return null;

            if (request.Application.AllotmentAccountNumber is "000" or null)
                return null;

            foreach (var bmgAllotment in bmgAllotments)
            {
                if (bmgAllotment.Value != request.Application.AmountOfPayment)
                    continue;

                string accountNumber = string.Empty;
                string routingNumber = string.Empty;

                if (bmgAllotment.AccountNumber != null)
                    accountNumber = Regex.Match(bmgAllotment.AccountNumber, @"\d+").Value;

                if (bmgAllotment.RoutingNumber != null)
                    routingNumber = Regex.Match(bmgAllotment.RoutingNumber, @"\d+").Value;

                if (request.Application.AllotmentRoutingNumber.EndsWith(routingNumber)
                    && request.Application.AllotmentAccountNumber.EndsWith(accountNumber))
                {
                    return bmgAllotment;
                }
            }

            return null;
        }

        private static void CheckAllotmentNecessity(AutoApprovalRequest request, AutoApprovalResponse response, AutoApprovalRule ruleResult)
        {
            bool isOpenPayrollMandatory = request.CreditPolicy.EmployerRules.EmployerRulesItems
                .Where(x => (x.Key == "open_payroll_mandatory" && x.Required) || x.Key == NewCreditPolicyRule.OpenPayrollMandatory)
                .FirstOrDefault()
                != null;

            var openPayrollConnection = request.OpenPayroll?.Connections.Any(o => o.IsActive);

            if (!isOpenPayrollMandatory)
            {
                AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.FirstLoan, AutoApprovalResultEnum.PendingAllotment);
                return;
            }
            else if (openPayrollConnection != null)
            {
                AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.FirstLoan, AutoApprovalResultEnum.PendingAllotment);
                return;
            }
            AutoApprovalResult.SetAutoApprovalResult(ruleResult, response, AllotmentRuleConstants.OpenPayrollMandatory, AutoApprovalResultEnum.Ignored);
        }
    }
}