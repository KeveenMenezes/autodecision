using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Models;

namespace AutodecisionCore.DTOs
{
    public class ApplicationFlagDto
    {
        public string FlagCode { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovalNote { get; set; }
        public string FlagName { get; set; }
        public string FlagDescription { get; set; }
        public virtual ICollection<ApplicationFlagsInternalMessage> ApplicationFlagsInternalMessage { get; set; } = new List<ApplicationFlagsInternalMessage>();

        public ApplicationFlagDto(ApplicationFlag flag, List<Flag> flags)
        {
            FlagCode = flag.FlagCode;
            Status = flag.Status;
            FlagDescription = flag.Description;
            ApplicationFlagsInternalMessage = flag.ApplicationFlagsInternalMessage;
            FlagName = flags.Where(flag => flag.Code == this.FlagCode).Select(fl => fl.Name).First();
        }
        public ApplicationFlagDto() { }
    }
}