using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.DTOs;
using AutodecisionCore.Events;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task<Application?> GetApplicationInfo(string loanNumber);
        Task<Application?> GetApplicationInfoByCustomerId(int customerId);
        Task<List<LastApplication>> GetLastApplications(int customerId, string loanNumber);
        Task<bool> ApproveApplication(ApproveApplicationRequestEvent request);
        Task<bool> DeclineApplication(DeclineApplicationRequestEvent request);
        Task<bool> RequireDocuments(RequireDocumentsRequestEvent request);
        Task NotifyDefaultDocuments(NotifyDefaultDocumentsRequestEvent request);
        Task<bool> RequireAllotment(RequireAllotmentRequestEvent request);

        Task<TotalIncome> GetTotalIncomeDetailsByApplicationIdAsync(int applicationId);
    }
}
