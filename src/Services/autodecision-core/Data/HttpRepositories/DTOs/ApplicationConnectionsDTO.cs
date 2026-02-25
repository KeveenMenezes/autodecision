namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class ApplicationConnectionsDTO
    {
        public int application_id { get; set; }
        public int? open_banking_id { get; set; }
        public int? open_payroll_id { get; set; }
        public long? face_id { get; set; }
        public int? debit_card_id { get; set; }
    }
}
