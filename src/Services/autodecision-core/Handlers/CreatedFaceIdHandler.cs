using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class CreatedFaceIdHandler : IConsumer<CreatedFaceIdRecordEvent>
    {
        private readonly ILogger<CreatedFaceIdHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IHandlerHelper _handlerHelper;

        public CreatedFaceIdHandler(
            ILogger<CreatedFaceIdHandler> logger,
            IApplicationRepository applicationRepository,
            IAutodecisionPublisherService autodecisionPublisherService,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _autodecisionPublisherService = autodecisionPublisherService;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<CreatedFaceIdRecordEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.CustomerId, "Created Face Id Event Handler without CustomerId")) return;

            var application = await _applicationRepository.GetApplicationInfoByCustomerId(context.Message.CustomerId);
            if (!_handlerHelper.IsApplicationProcessable(application, context.Message.CustomerId)) return;

            await _autodecisionPublisherService.PublishAutodecisionProcessRequest(application, Reason.CreatedFaceId);
            _logger.LogInformation($"LoanNumber: {application.LoanNumber} - Created Face Id Event - request Autodecision Process has been sent.");
        }
    }
}