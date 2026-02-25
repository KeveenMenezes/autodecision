using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;

namespace AutodecisionCore.Services.Interfaces
{
    public interface ICreditRiskService
    {
        bool ValidateAllowedLoanToCalculateScore(Application application, Employer employer);
        bool ValidateDocAllotmentSDD(Application application, Employer employer, List<ApplicationDocuments>? applicationDocuments = null);
    }
}
