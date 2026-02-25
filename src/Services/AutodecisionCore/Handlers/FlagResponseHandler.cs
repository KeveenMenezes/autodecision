using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services;
using AutodecisionCore.Services.Interfaces;
using MassTransit;
using System.Diagnostics;

namespace AutodecisionCore.Handlers
{
    public class FlagResponseHandler : IConsumer<ProcessFlagResponseEvent>
    {
        private readonly ILogger<FlagResponseHandler> _logger;
        private readonly IApplicationFlagsService _applicationFlagsService;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly IHandlerHelper _handlerHelper;

        public FlagResponseHandler(
            ILogger<FlagResponseHandler> logger,
            IApplicationFlagsService applicationFlagsService,
            IAutodecisionPublisherService autodecisionPublisherService,
            IApplicationCoreRepository applicationCoreRepository,
            IHandlerHelper handlerHelper
        )
        {
            _logger = logger;
            _applicationFlagsService = applicationFlagsService;
            _autodecisionPublisherService = autodecisionPublisherService;
            _applicationCoreRepository = applicationCoreRepository;
            _handlerHelper = handlerHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagResponseEvent> context)
        {
            if (!IsFlagResponseValid(context)) return;

            var timer = new Stopwatch();
            timer.Start();

            var applicationCore = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(context.Message.LoanNumber);
            if (!IsValidVersion(context.Message, applicationCore.ProcessingVersion)) return;

            await _applicationFlagsService.FlagResponseStatusRegister(context.Message, applicationCore);
            await _autodecisionPublisherService.PublishFinalEvaluationRequest(context.Message);

            timer.Stop();
            _logger.LogInformation($"Total time FlagResponseHandler: {timer.Elapsed.ToString(@"m\:ss\.fff")}. Loan Number: {context.Message.LoanNumber}");
        }

        private bool IsFlagResponseValid(ConsumeContext<ProcessFlagResponseEvent> context)
        {
            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Process Flag Response Event Handler without LoanNumber")) return false;
            if (!_handlerHelper.ValidateProperty(context.Message.FlagCode, $"Loan Number: {context.Message.LoanNumber} received a flag response without FlagCode")) return false;

            return true;
        }

        private bool IsValidVersion(ProcessFlagResponseEvent message, int applicationCoreVersion)
        {
            if (message.Version.HasValue)
            {
                if (message.Version != applicationCoreVersion)
                {
                    _logger.LogInformation($"Rejected. Old processing version. Loan Number: {message.LoanNumber}");
                    return false;
                }
            }
            return true;
        }
    }
}
