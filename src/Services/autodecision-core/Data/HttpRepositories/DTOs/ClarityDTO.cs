namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class ClarityDTO
    {
        public string DobMatchDescription { get; set; }
        public string DobMatchResult { get; set; }
        public int? NameAddressMatchConfidence { get; set; }
        public string NameAddressMatchDescription { get; set; }
        public int? SSNNameAddressMatchConfidence { get; set; }
        public string SSNNameAddressMatchDescription { get; set; }
        public string RequestHash { get; set; }
        public bool OFACHit { get; set; }   
    }
}
