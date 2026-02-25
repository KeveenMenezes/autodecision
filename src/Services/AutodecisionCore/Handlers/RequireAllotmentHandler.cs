using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using MassTransit;

namespace AutodecisionCore.Handlers
{
    public class RequireAllotmentHandler : IConsumer<RequireAllotmentRequestEvent>
    {
        private readonly ILogger<RequireDocumentsRequestEvent> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IHandlerHelper _handlerHelper;

        public RequireAllotmentHandler(
            ILogger<RequireDocumentsRequestEvent> logger, 
            IApplicationRepository applicationRepository,
            IHandlerHelper handlerHelper)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<RequireAllotmentRequestEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Require Allotment Request Event Handler without LoanNumber")) return;
            await _applicationRepository.RequireAllotment(context.Message);
            _logger.LogInformation($"LoanNumber: {context.Message.LoanNumber} - Require Allotment via CustomerInfo process has been sent.");
        }
    }
}