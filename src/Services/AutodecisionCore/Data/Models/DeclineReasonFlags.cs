using AutodecisionCore.Data.Models.Base;

namespace AutodecisionCore.Data.Models
{
    public class DeclineReasonFlags : BaseModel
    {
        public string FlagCode { get; set; } = string.Empty;
        public int ReasonId { get; set; }
    }
}
