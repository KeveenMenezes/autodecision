using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.AutoApprovalCore.DTO
{
	public class AutoApprovalResponse
	{
        public List<AutoApprovalRule> AutoApprovalRules { get; set; } = new List<AutoApprovalRule>();
    }
}
