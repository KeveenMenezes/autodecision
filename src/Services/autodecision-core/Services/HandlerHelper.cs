using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Services
{
    public class HandlerHelper : IHandlerHelper
    {
        private readonly ILogger<HandlerHelper> _logger;

        public HandlerHelper(ILogger<HandlerHelper> logger)
        {
            _logger = logger;
        }

        public bool IsApplicationProcessable(Application? application, int customerId)
        {
            if (application == null)
            {
                _logger.LogError($"Application not found for CustomerId: {customerId}");
                return false;
            }

            if (!IsStatusAllowed(application.Status))
            {
                _logger.LogInformation($"Application Status: '{application.Status}' for CustomerId: {customerId} is not allowed");
                return false;
            }

            return true;
        }

        public bool IsApplicationProcessable(Application? application, string loanNumber)
        {
            if (application == null)
            {
                _logger.LogError($"Application not found for loanNumber: {loanNumber}");
                return false;
            }
            if (!IsStatusAllowed(application.Status))
            {
                _logger.LogInformation($"Application Status: '{application.Status}' for LoanNumber: {loanNumber} is not allowed");
                return false;
            }
            return true;
        }

        public bool ValidateProperty(int property, string message)
        {
            if (property == Constants.Zero)
            {
                _logger.LogInformation(message);
                return false;
            }
            return true;
        }

        public bool ValidateProperty(string property, string message)
        {
            if (string.IsNullOrEmpty(property))
            {
                _logger.LogWarning(message);
                return false;
            }
            return true;
        }

        public bool HasRedLockKeyBeenAcquired(bool isAcquired, string resource, string loanNumber, string processName)
        {
            if (isAcquired) return true;
            _logger.LogWarning($"{processName}: Resource {resource} is locked. LoanNumber: {loanNumber}");
            throw new Exception($"Red Lock don't acquired for LoanNumber: {loanNumber}");
        }

        public bool IsRedisAvailable(string status) =>
            status switch
            {
                ApplicationStatus.Processing => true,
                ApplicationStatus.PendingApproval => true,
                ApplicationStatus.PendingSignature => true,
                ApplicationStatus.Signed => true,
                ApplicationStatus.Booked => true,
                ApplicationStatus.OpenedForChanges => true,
                _ => false
            };

        private static bool IsStatusAllowed(string status) =>
            status switch
            {
                ApplicationStatus.Processing => true,
                ApplicationStatus.PendingApproval => true,
                ApplicationStatus.OpenedForChanges => true,
                _ => false
            };
    }
}