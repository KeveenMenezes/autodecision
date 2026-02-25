using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Services.Interfaces
{
	public interface IAutoApprovalService
	{
		Task RunAutoApproval(ApplicationCore applicationCore, AutodecisionCompositeData autodecisionCompositeData, string loanNumber, string key);
	}
}
