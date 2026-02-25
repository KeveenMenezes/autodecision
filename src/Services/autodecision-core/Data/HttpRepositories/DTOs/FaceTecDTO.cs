namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class FaceTecDTO
    {
        public string Status { get; set; }
        public ResponseBase Enrollment { get; set; }
        public List<ResponseBase> Enrollments { get; set; }
        public DocumentScan DocumentScan { get; set; }
    }

    public class FacetecResponseApi
    {
        public FaceTecDTO Response { get; set; }
    }

    public class ResponseBase
    {
        public string Status { get; set; }
        public bool Success { get; set; }
        public bool Liveness { get; set; }
        public int CustomerId { get; set; }
    }

    public class DocumentScan
    {
        public bool Success { get; set; }
        public DocumentData DocumentData { get; set; }
    }

    public class DocumentData
    {
        public string IdType { get; set; }

        public ScannedData Nfc { get; set; }

        public ScannedData Barcode { get; set; }

        public ScannedData Scanned { get; set; }

        public ScannedData UserConfirmed { get; set; }
    }

    public class ScannedData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DateOfBirth { get; set; }

        public string Address { get; set; }

        public string Sex { get; set; }

        public string IdNumber { get; set; }
    }
}