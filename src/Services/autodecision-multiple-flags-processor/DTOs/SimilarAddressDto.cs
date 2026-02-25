namespace AutodecisionMultipleFlagsProcessor.DTOs
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class SimilarAddressDto
    {
        public int CustomerId { get; set; }
        public string LoanNumber { get; set; }
        public string StreetAddress { get; set; }
        public string UnitNumber { get; set; }
        public string CityName { get; set; }
        public string StateAbbreviation { get; set; }
        public string Zipcode { get; set; }
        public int LevenshteinDistance { get; set; }
        public decimal Similarity { get; set; }

        public SimilarAddressDto()
        {

        }

        public SimilarAddressDto(int customerId, string loanNumber, string streetAddress, string unitNumber, string cityName, string stateAbbreviation, string zipcode, int levenshteinDistance, decimal similarity)
        {
            LoanNumber = loanNumber;
            StreetAddress = streetAddress;
            UnitNumber = unitNumber;
            CityName = cityName;
            StateAbbreviation = stateAbbreviation;
            Zipcode = zipcode;
            LevenshteinDistance = levenshteinDistance;
            Similarity = similarity;
            CustomerId = customerId;
        }
    }
}
