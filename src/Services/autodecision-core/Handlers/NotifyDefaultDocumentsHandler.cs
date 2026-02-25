using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class NotifyDefaultDocumentsHandler : IConsumer<NotifyDefaultDocumentsRequestEvent>
    {
        private readonly ILogger<NotifyDefaultDocumentsHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IHandlerHelper _handlerHelper;

        public NotifyDefaultDocumentsHandler(
            ILogger<NotifyDefaultDocumentsHandler> logger, 
            IApplicationRepository applicationRepository,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<NotifyDefaultDocumentsRequestEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Notify Default Documents Request Event Handler without LoanNumber")) return;
            await _applicationRepository.NotifyDefaultDocuments(context.Message);
            _logger.LogInformation($"LoanNumber: {context.Message.LoanNumber} - Notify Default Documents via CustomerInfo process has been sent.");
        }
    }
}