using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IExternalValidationService
    {
        Task Run(Application application);
    }
}
