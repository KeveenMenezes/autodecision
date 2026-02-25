using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class RequireDocumentsHandler : IConsumer<RequireDocumentsRequestEvent>
    {
        private readonly ILogger<RequireDocumentsHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IHandlerHelper _handlerHelper;

        public RequireDocumentsHandler(
            ILogger<RequireDocumentsHandler> logger, 
            IApplicationRepository applicationRepository,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<RequireDocumentsRequestEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Require Documents Request Event Handler without LoanNumber")) return;
            await _applicationRepository.RequireDocuments(context.Message);
            _logger.LogInformation($"LoanNumber: {context.Message.LoanNumber} - Require Documents via CustomerInfo process has been sent.");
        }
    }
}