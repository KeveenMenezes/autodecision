using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Services.Interfaces
{
	public interface IFinalValidationService
	{
		Task Process(ProcessFinalValidation message, ApplicationCore applicationCore);
	}
}
