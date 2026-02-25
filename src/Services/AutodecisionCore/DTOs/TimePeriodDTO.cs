namespace AutodecisionCore.DTOs
{
    public class TimePeriodDTO
    {
        public int Id{ get; set; }
        public string Description { get; set; }
        public int UnitTime { get; set; }
        public int Interval { get; set; }
        public bool IsDefault { get; set; }
    }
}
