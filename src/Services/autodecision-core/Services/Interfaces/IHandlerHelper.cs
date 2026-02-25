using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IHandlerHelper
    {
        bool IsApplicationProcessable(Application? application, int customerId);
        bool IsApplicationProcessable(Application? application, string loanNumber);
        bool ValidateProperty(int property, string message);
        bool ValidateProperty(string property, string message);
        bool HasRedLockKeyBeenAcquired(bool isAcquired, string resource, string loanNumber, string processName);
        bool IsRedisAvailable(string status);
    }
}