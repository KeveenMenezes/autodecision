using AutodecisionCore.Data.Models.Base;

namespace AutodecisionCore.Data.Models
{
    public class TimePeriod : BaseModel
    {
        public string Description { get; set; }
        public int UnitTime { get; set; }
        public int Interval { get; set; }
        public bool IsDefault { get; set; }
    }
}
