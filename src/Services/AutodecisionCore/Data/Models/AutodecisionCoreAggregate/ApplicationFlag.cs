using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Extensions;
using MassTransit;

namespace AutodecisionCore.Data.Models.AutodecisionCoreAggregate
{
    public class ApplicationFlag : BaseModel
    {
        public string FlagCode { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovalNote { get; set; }
        public bool InternalFlag { get; set; }
        public virtual ICollection<ApplicationFlagsInternalMessage> ApplicationFlagsInternalMessage { get; set; } = new List<ApplicationFlagsInternalMessage>();

        public List<ApplicationFlagsInternalMessage> GetApplicationFlagsInternalMessage() =>
            ApplicationFlagsInternalMessage.ToList();

        public bool IsFlagCodeRelatedToAutoApproval()
        {
            return FlagCode == Extensions.FlagCode.LoanVerification || FlagCode == Extensions.FlagCode.Flag209; 
        }
        
    }
}
