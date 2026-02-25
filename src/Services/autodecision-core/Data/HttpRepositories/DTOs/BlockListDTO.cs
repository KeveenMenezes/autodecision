namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class BlockListDTO
    {
        public string Reason { get; set; }
        public string CreatedNote { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
