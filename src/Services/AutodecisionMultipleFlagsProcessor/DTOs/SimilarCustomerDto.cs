namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class SimilarCustomerDto
    {
        public string loan_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string ssn { get; set; }
        public int levenshtein_distance { get; set; }
        public decimal ssn_similarity { get; set; }
        public decimal name_similarity { get; set; }
    }
}
