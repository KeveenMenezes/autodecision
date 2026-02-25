namespace AutodecisionCore.DTOs
{
    public class CountProcessingFlagDTO
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int TimeConsumedToProcess { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int CountFlag { get; set; }
        public int FlagCode { get; set; }
    }
}
