using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class ConnectedDebitCardHandler : IConsumer<ConnectedDebitCardEvent>
    {
        private readonly ILogger<ConnectedDebitCardHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IHandlerHelper _handlerHelper;

        public ConnectedDebitCardHandler(
            ILogger<ConnectedDebitCardHandler> logger,
            IApplicationRepository applicationRepository,
            IAutodecisionPublisherService autodecisionPublisherService,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _autodecisionPublisherService = autodecisionPublisherService;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<ConnectedDebitCardEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.CustomerId, "Connected Debit Card Event Handler without CustomerId")) return;

            var application = await _applicationRepository.GetApplicationInfoByCustomerId(context.Message.CustomerId);
            if (!_handlerHelper.IsApplicationProcessable(application, context.Message.CustomerId)) return;

            await _autodecisionPublisherService.PublishAutodecisionProcessRequest(application, Reason.GetDebitCardReason(context.Message.DebitCardEventType));
            _logger.LogInformation($"LoanNumber: {application.LoanNumber} - Connected Debit Card Event - request Autodecision Process has been sent");
        }
    }
}