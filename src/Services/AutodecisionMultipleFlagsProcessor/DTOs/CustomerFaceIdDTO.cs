namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class CustomerFaceIdDTO
    {
        public Response Response { get; set; }
        public bool Success { get; set; }
        public bool Error { get; set; }
        public object ErrorDetail { get; set; }
    }

    public class Response
    {
        public string Status { get; set; }
        public List<Enrollment> Enrollments { get; set; }
    }
    public class Enrollment
    {
        public int CustomerId { get; set; }
        public string Status { get; set; }
        public string Selfie { get; set; }
        public int MatchLevel { get; set; }
        public string MatchLevelDescription { get; set; }
    }
}
