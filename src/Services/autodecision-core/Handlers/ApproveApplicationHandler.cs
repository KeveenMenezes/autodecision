using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class ApproveApplicationHandler : IConsumer<ApproveApplicationRequestEvent>
    {
        private readonly ILogger<ApproveApplicationHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IHandlerHelper _handlerHelper;

        public ApproveApplicationHandler(
            ILogger<ApproveApplicationHandler> logger, 
            IApplicationRepository applicationRepository,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<ApproveApplicationRequestEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Approve Application Request Event Handler without LoanNumber")) return;
            await _applicationRepository.ApproveApplication(context.Message);
            _logger.LogInformation($"LoanNumber: {context.Message.LoanNumber} - Approve Application via CustomerInfo process has been sent.");
        }
    }
}
