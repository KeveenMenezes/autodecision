using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Core.AutoApprovalCore.Interface
{
	public interface IAutoApprovalItem
	{
		public Task RunRule(AutoApprovalRequest request, AutoApprovalResponse response, ApplicationCore applicationCore);
	}
}
