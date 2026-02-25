using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Data.Models.AutodecisionCoreAggregate
{
    public class AutoApprovalRule : BaseModel
    {
        public string RuleName { get; set; }
        public AutoApprovalResultEnum Status { get; set; }
        public string Description { get; set; }
    }
}
