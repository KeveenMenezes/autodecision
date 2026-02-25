using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
	public class CreditPolicyRule : IAutoApprovalItem
	{
		private readonly IFeatureToggleClient _featureToggleClient;
		private readonly ILogger<AutoApprovalManager> _logger;

		public CreditPolicyRule(
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
				RuleName = "CreditPolicyRule",
			};

			try
			{
				if (_featureToggleClient.IsDisabled("AutoApprovalRuleCreditPolicy"))
					return Task.CompletedTask;

				if (request.CreditPolicy.EmployerRules.EmployerRulesItems.Count > 0)
				{
					ruleResult.Status = AutoApprovalResultEnum.Approved;
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				ruleResult.Status = AutoApprovalResultEnum.Error;
				ruleResult.Description = "Credit Policy not found.";
				response.AutoApprovalRules.Add(ruleResult);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: CreditPolicyRule | Error: {ex.Message}");

				ruleResult.Status = AutoApprovalResultEnum.Error;
				ruleResult.Description = $"Rule Name: CreditPolicyRule | Error: {ex.Message} ";
				response.AutoApprovalRules.Add(ruleResult);
				return Task.CompletedTask;
			}
		}
	}
}
