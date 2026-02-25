using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using System.Text.RegularExpressions;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class OpenBankingOpenPayrollBankAccountRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public OpenBankingOpenPayrollBankAccountRule(
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
                RuleName = "OpenBankingOpenPayrollBankAccountRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleOpenBankingOpenPayrollBankAccount"))
                    return Task.CompletedTask;

                if (request.Application.Program == BMGMoneyProgram.LoansAtWork)
                {
                    return Task.CompletedTask;
                }

                var flagsWithDocumentsApproved = HasFlagsWithDocumentsApproved(applicationCore)
                                                    .Select(x => x.Description).ToList() ?? new List<string>();

                if (flagsWithDocumentsApproved.Any())
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = string.Join(", ", flagsWithDocumentsApproved);
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (request.OpenPayroll.Connections == null)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"OpenPayroll connections not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (request.OpenBanking.Connections == null)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"OpenBanking connections not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                var payAllocations = request.OpenPayroll.Connections
                    .SelectMany(x => x.PayAllocations)
                    .Where(x => x.IsRemainder)
                    .ToList();

                var openBankingConnection = request.OpenBanking.Connections.FirstOrDefault(x => x.IsDefault);


                if (payAllocations == null || !payAllocations.Any())
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Remainder Pay Allocation not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (openBankingConnection == null)
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Open banking not found.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                foreach (var item in payAllocations)
                {
                    if (string.IsNullOrEmpty(item.RoutingNumber) || string.IsNullOrEmpty(item.AccountNumber))
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = $"Pay Allocation account/routing number not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    if (string.IsNullOrEmpty(openBankingConnection.RoutingNumber) || string.IsNullOrEmpty(openBankingConnection.AccountNumber))
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Pending;
                        ruleResult.Description = $"Open Banking account/routing number not found.";
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }

                    var accountNumber = Regex.Match(item.AccountNumber, @"\d+").Value;
                    var routingNumber = Regex.Match(item.RoutingNumber, @"\d+").Value;

                    if (openBankingConnection.RoutingNumber.EndsWith(routingNumber)
                        && openBankingConnection.AccountNumber.EndsWith(accountNumber))
                    {
                        ruleResult.Status = AutoApprovalResultEnum.Approved;
                        response.AutoApprovalRules.Add(ruleResult);
                        return Task.CompletedTask;
                    }
                }

                ruleResult.Status = AutoApprovalResultEnum.Pending;
                ruleResult.Description = $"No match found between OpenPayroll and OpenBanking bank accounts.";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: OpenBankingOpenPayrollBankAccountRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: OpenBankingOpenPayrollBankAccountRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
        }

        private List<ApplicationFlag> HasFlagsWithDocumentsApproved(ApplicationCore applicationCore)
        {
            return new List<ApplicationFlag>
            {
                applicationCore.HasBankStatementApproved(),
                applicationCore.HasPaystubApproved()
            }.Where(flag => flag != null).ToList();
        }
    }
}
