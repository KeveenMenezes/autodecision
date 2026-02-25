using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
	public class FaceRecognitionRule : IAutoApprovalItem
	{
		private readonly IFeatureToggleClient _featureToggleClient;
		private readonly ILogger<AutoApprovalManager> _logger;

		public FaceRecognitionRule(
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
				RuleName = "FaceRecognitionRule",
			};

			try
			{
				if (_featureToggleClient.IsDisabled("AutoApprovalRuleFaceRecognition"))
					return Task.CompletedTask;

				if (request.FaceRecognition.EnrollmentStatus != "DONE")
				{
					ruleResult.Status = AutoApprovalResultEnum.Pending;
					ruleResult.Description = $"Enrollment Status is different from \"DONE\".";
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				if (!(request.FaceRecognition.Liveness ?? false))
				{
					ruleResult.Status = AutoApprovalResultEnum.Pending;
					ruleResult.Description = $"Liveness failed or not found.";
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				if (!(request.FaceRecognition.DocumentScanSuccess ?? false))
				{
					ruleResult.Status = AutoApprovalResultEnum.Pending;
					ruleResult.Description = $"Document scan failed or not found.";
					response.AutoApprovalRules.Add(ruleResult);
					return Task.CompletedTask;
				}

				ruleResult.Status = AutoApprovalResultEnum.Approved;
				response.AutoApprovalRules.Add(ruleResult);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: FaceRecognitionRule | Error: {ex.Message}");

				ruleResult.Status = AutoApprovalResultEnum.Error;
				ruleResult.Description = $"Rule Name: FaceRecognitionRule | Error: {ex.Message} ";
				response.AutoApprovalRules.Add(ruleResult);
				return Task.CompletedTask;
			}
		}
	}
}
