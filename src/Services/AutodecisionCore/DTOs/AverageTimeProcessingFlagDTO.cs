namespace AutodecisionCore.DTOs
{
    public class AverageTimeProcessingFlagDTO
    {
        public string Range { get; set; }
        public decimal Value { get; set; }
        public FlagAverageTimeDTO Flag { get; set; }
    }

    public class FlagAverageTimeDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } 
    }


}
