using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IApplicationFlagsService
    {
        Task AddApplicationFlagsToRegister(ApplicationCore applicationCore);
        Task AddOnlyInternalApplicationFlagsToRegister(ApplicationCore applicationCore);
        Task AddApplicationFlagsToRegisterByIgnoringAsync(ApplicationCore applicationCore, string ignoredFlagCode, string description);
        Task AddApplicationFlagsToRegisterByIgnoringAsync(ApplicationCore applicationCore, string[] ignoredFlagCodes, string description);
        Task FlagResponseStatusRegister(ProcessFlagResponseEvent response, ApplicationCore applicationCore);
        Task HandleApplicationFlags(Application application, ApplicationCore applicationCore, string reason);
        Task BindApplicationFlagsByTriggerAsync(ApplicationCore applicationCore, string reason);
    }
}