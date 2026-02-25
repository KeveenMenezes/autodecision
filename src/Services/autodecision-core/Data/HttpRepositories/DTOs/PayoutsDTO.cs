namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class PayoutsDTO
    {
        public decimal NetPay { get; set; }
        public decimal GrossPay { get; set; }
        public DateTime PayDate { get; set; }
        public string Employer { get; set; }
    }
}
