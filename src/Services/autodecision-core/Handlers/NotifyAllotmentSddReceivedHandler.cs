using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class NotifyAllotmentSddReceivedHandler : IConsumer<NotifyAllotmentSddReceivedEvent>
    {
        private readonly ILogger<NotifyAllotmentSddReceivedHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IHandlerHelper _handlerHelper;

        public NotifyAllotmentSddReceivedHandler(
            ILogger<NotifyAllotmentSddReceivedHandler> logger,
            IApplicationRepository applicationRepository,
            IAutodecisionPublisherService autodecisionPublisherService,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _autodecisionPublisherService = autodecisionPublisherService;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<NotifyAllotmentSddReceivedEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Notify Allotment SDD Received Event Handler without LoanNumber")) return;

            var application = await _applicationRepository.GetApplicationInfo(context.Message.LoanNumber);
            if (!_handlerHelper.IsApplicationProcessable(application, context.Message.LoanNumber)) return;

            await _autodecisionPublisherService.PublishAutodecisionProcessRequest(application, Reason.AllotmentSDDReceived);
            _logger.LogInformation($"LoanNumber: {application.LoanNumber} - Notify Allotment SDD Received Handler - request Autodecision Process process has been sent.");
        }
    }
}