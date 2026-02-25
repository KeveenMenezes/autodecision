using AutodecisionCore.Commands;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.DTOs;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IApplicationCoreService
    {
        Task<ApplicationCore> ApplicationCoreRegister(string loanNumber);
        Task<List<ApplicationDto>> GetFlagsByLoanNumbers(List<string> loanNumbers);
        Task<List<ApplicationFlagDto>> GetFlagsByLoanNumber(string loanNumbers);
        Task<ApplicationFlagDto> GetFlagsByLoanNumberAndFlagCode(string loanNumber, string flagCode);
        Task<List<ApplicationFlagDto>> ApproveFlag(string loanNumber, string flagCode, ApproveFlagCommand command);
		Task<ResultDTO> OpenForChanges(string loanNumber, string user);
	}
}