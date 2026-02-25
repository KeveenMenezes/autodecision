namespace AutodecisionCore.DTOs
{
    public class AmountApplicatonByStatusResponseDTO
    {

        public Details Pending { get; set; }
        public Details AutoDeny { get; set; }
        public Details AutoApproval { get; set; }
        public Details PendingApproval { get; set; }
        public Details PendingDocuments { get; set; }
    }

    public class Details
    {
        public int Value { get; set; }
        public string Percent { get; set; }
    }
}
