using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.AutoApprovalCore.Interface
{
	public interface IAutoApprovalManager
	{
		Task RunRule(AutoApprovalRequest request, ApplicationCore applicationCore);
	}
}
