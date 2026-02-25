using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Services.Interfaces
{
    public interface ICustomerInfo
    {
        Task<IEnumerable<ApplicationDto>> GetOtherApplicationsWithSameFingerprint(string browserFingerprint, int customerId);
        Task<OtherOpenLoanDTO> GetOtherOpenLoan(int applicationid, int customerid, string type);
        Task<ApplicationsWithSameBankInfoDTO> GetApplicationsWithSameBankInfo(int customer_id, string bank_routing_number, string bank_account_number);
        Task<bool> CheckIfRoutingNumberAlreadyExists(string routingNumber);
        Task<List<SimilarPhoneDataDTO>> GetCustomersWithSamePhone(int customerId, string PhoneNumber, string SecondaryPhoneNumber, string WorkPhoneNumber);
        Task<bool> GetDailyReceivings(int customerId);
        Task<List<SimilarCustomerDto>> GetSimilarCustomers(int customerId);
        Task<bool> CheckFirstnetCredit(int customerId, int daysCheck);
        PacerValidationDto ValidateBankruptcy(int applicationId);
        Task<GiactResultDto> GetLastGiactResult(string loanNumber);
        ApplicationDto GetPreviousApplication(int applicationId);
        Task<CensusDTO> GetCensusByCustomerIdWithCriteria(int employerId, int customerId, string criteria);
        Task<bool> CheckHasBook(List<SimilarAddressDto> similar_address_list);
        Task<bool> IsWhitelistRelated(int customerId, int customerIdRelated);
        Task<List<SimilarAddressDto>> CheckCustomersWithSameAddressAsync(SimilarAddressDto request);
        Task<List<SimilarAddressDto>> GetSimilarAddressListForCustomerAsync(int customerId, int levenshteinDistance);
    }
}
