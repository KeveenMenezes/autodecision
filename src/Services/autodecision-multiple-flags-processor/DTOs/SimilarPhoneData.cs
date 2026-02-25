namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class SimilarPhoneDataDTO
    {
        public string LoanNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public SimilarPhoneDataDTO(string LoanNumber, string FirstName, string LastName)
        {
            this.LoanNumber = LoanNumber;
            this.FirstName = FirstName;
            this.LastName = LastName;
        }

        public SimilarPhoneDataDTO()
        {

        }

    }
}