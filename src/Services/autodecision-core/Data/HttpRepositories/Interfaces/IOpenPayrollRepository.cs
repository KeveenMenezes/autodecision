using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IOpenPayrollRepository
    {
        Task<OpenPayroll> GetOpenPayrollConnections(int customerId, int applicationId, DateTime submittedAt);
    }
}
