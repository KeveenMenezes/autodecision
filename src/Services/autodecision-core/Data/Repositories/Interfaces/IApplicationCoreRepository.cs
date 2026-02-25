using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Base;

namespace AutodecisionCore.Data.Repositories.Interfaces
{
    public interface IApplicationCoreRepository : IBaseRepository<ApplicationCore>
    {
        Task<ApplicationCore> FindByLoanNumberAsync(string loanNumber);
        Task<ApplicationCore> FindByLoanNumberIncludeApplicationFlagsAsync(string loanNumber);
        Task<ApplicationCore> FindByLoanNumberIncludeApplicationProcessAsync(string loanNumber);
        Task<ApplicationCore> FindByLoanNumberIncludeAutoApprovalRulesAsync(string loanNumber);
        Task<Dictionary<string, List<ApplicationFlag>>> FindByLoanNumbersIncludeApplicationFlagsAsync(List<string> loanNumbers);
        Task<bool> HasCustomerIdentityFlagNotRaised(string loanNumber);
    }
}
