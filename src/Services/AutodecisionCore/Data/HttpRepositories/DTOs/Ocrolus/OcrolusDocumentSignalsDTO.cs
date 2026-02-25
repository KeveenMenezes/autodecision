namespace AutodecisionCore.Data.HttpRepositories.DTOs.Ocrolus
{
    public class OcrolusDocumentSignalsDTO
    {
        public int Id { get; set; }
        public int CustomerIdentifierId { get; set; }
        public string? Reason { get; set; }
        public string? ConfidenceLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}