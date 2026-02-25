using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Data.Models.AutodecisionCoreAggregate
{
    public class ApplicationProcess : BaseModel
    {
        public int ProcessingVersion { get; set; }
        public InternalStatusEnum Status { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
