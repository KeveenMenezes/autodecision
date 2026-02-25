using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Utils
{
    public class AutoApprovalResult
    {
        public static void SetAutoApprovalResult(AutoApprovalRule ruleResult, AutoApprovalResponse response, string description, AutoApprovalResultEnum status = AutoApprovalResultEnum.Pending)
        {
            ruleResult.Status = status;
            ruleResult.Description = description;
            response.AutoApprovalRules.Add(ruleResult);
        }
    }
}
