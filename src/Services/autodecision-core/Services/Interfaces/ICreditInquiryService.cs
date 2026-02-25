using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Services.Interfaces
{
    public interface ICreditInquiryService
    {
        Task RunClarityInquiry(Application application);
        Task RunGiact(Application application);
        Task RunFedsDataCenter(Application application);
        Task RunEquifaxWorkNumberInquiry(Application application);
        Task RunFactorTrustInquiry(Application application);
        Task StartTransunionProcess(Application application);
    }
}
