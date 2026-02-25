using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.Interface;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Core.AutoApproval.Rules
{
    public class FaceRecognitionOnexNRule : IAutoApprovalItem
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<AutoApprovalManager> _logger;

        public FaceRecognitionOnexNRule(
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
                RuleName = "FaceRecognitionOnexNRule",
            };

            try
            {
                if (_featureToggleClient.IsDisabled("AutoApprovalRuleFaceRecognitionOnexN"))
                    return Task.CompletedTask;

                if (request.FaceRecognition.FraudStatus != "DONE")
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"FraudStatus is different from \"DONE\".";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                if (request.FaceRecognition.ClientIdsMatch.Any(x => x > 0))
                {
                    ruleResult.Status = AutoApprovalResultEnum.Pending;
                    ruleResult.Description = $"Other related facial recognition has been identified.";
                    response.AutoApprovalRules.Add(ruleResult);
                    return Task.CompletedTask;
                }

                ruleResult.Status = AutoApprovalResultEnum.Approved;
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: 180/209 was not successfully processed for LoanNumber: {request.LoanNumber} | Rule Name: FaceRecognitionOnexNRule | Error: {ex.Message}");

                ruleResult.Status = AutoApprovalResultEnum.Error;
                ruleResult.Description = $"Rule Name: FaceRecognitionOnexNRule | Error: {ex.Message} ";
                response.AutoApprovalRules.Add(ruleResult);
                return Task.CompletedTask;
            }
        }
    }
}
