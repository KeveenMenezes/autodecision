using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class DeclineApplicationHandler : IConsumer<DeclineApplicationRequestEvent>
    {
        private readonly ILogger<DeclineApplicationHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IHandlerHelper _handlerHelper;

        public DeclineApplicationHandler(
            ILogger<DeclineApplicationHandler> logger,
            IApplicationRepository applicationRepository,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<DeclineApplicationRequestEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Decline Application Request Event Handler without LoanNumber")) return;
            await _applicationRepository.DeclineApplication(context.Message);
            _logger.LogInformation($"LoanNumber: {context.Message.LoanNumber} Decline Application via CustomerInfo process has been sent.");
        }
    }
}