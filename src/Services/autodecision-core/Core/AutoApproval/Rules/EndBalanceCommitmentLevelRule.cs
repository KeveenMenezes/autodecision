using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
	public class EndBalanceCommitmentLevelRule : IAutoApprovalItem
	{
		private readonly IFeatureToggleClient _featureToggleClient;
		private readonly ILogger<AutoApprovalManager> _logger;

		public EndBalanceCommitmentLevelRule(
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
				RuleName = "EndBalanceCommitmentLevelRule",
			};

			try
			{
				if (_featureToggleClient.IsDisabled("AutoApprovalRuleEndBalanceCommitmentLevel"))
					return Task.CompletedTask;

				if (request.CreditPolicy.EmployerRules.EmployerRulesItems == null)
				{
					ruleResult.Status = AutoApprovalResultEnum.Pending;
					ruleResult.Description = "Employer rules not found.";
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				var rule = request.CreditPolicy.EmployerRules.EmployerRulesItems
					.Where(x => x.Key == "end_balance_commitment_level" || x.Key == NewCreditPolicyRule.EndBalanceCommitmentLevel)
					.FirstOrDefault();
				
				if (rule == null)
				{
					ruleResult.Status = AutoApprovalResultEnum.Ignored;
					ruleResult.Description = "End Balance Commitment Level rule not defined.";
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				ruleResult.Status = AutoApprovalResultEnum.Approved;
				response.AutoApprovalRules.Add(ruleResult);

				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: EndBalanceCommitmentLevelRule | Error: {ex.Message}");

				ruleResult.Status = AutoApprovalResultEnum.Error;
				ruleResult.Description = $"Rule Name: EndBalanceCommitmentLevelRule | Error: {ex.Message} ";
				response.AutoApprovalRules.Add(ruleResult);
				return Task.CompletedTask;
			}
		}
	}
}
