using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using MassTransit;
using RedLockNet;
using System.Diagnostics;

namespace AutodecisionCore.Handlers
{
    public class FinalValidationHandler : IConsumer<ProcessFinalValidation>
	{
		private readonly ILogger<FinalValidationHandler> _logger;
		private readonly IDistributedLockFactory _redLockFactory;
		private readonly IFinalValidationService _finalValidationService;
		private readonly IApplicationCoreRepository _applicationCoreRepository;
		private readonly IHandlerHelper _handlerHelper;

		public FinalValidationHandler(
			ILogger<FinalValidationHandler> logger,
			IDistributedLockFactory redLockFactory,
			IFinalValidationService finalValidationService,
			IApplicationCoreRepository applicationCoreRepository,
			IHandlerHelper handlerHelper)
		{
			_logger = logger;
			_redLockFactory = redLockFactory;
			_finalValidationService = finalValidationService;
			_applicationCoreRepository = applicationCoreRepository;
			_handlerHelper = handlerHelper;
		}

		public async Task Consume(ConsumeContext<ProcessFinalValidation> context)
		{
			if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, " Process Final validation Handler without LoanNumber")) return;

			var timer = new Stopwatch();
			timer.Start();

			var loanNumber = context.Message.LoanNumber;
            var resource = $"{RedLockKeys.RequestProcess}{context.Message.LoanNumber}";
            var timeResouce = new TimeSpan(0, 0, 2, 0);
            var retryTime = new TimeSpan(0, 0, 0, 3);

            using (var redLock = await _redLockFactory.CreateLockAsync(resource, timeResouce, timeResouce, retryTime))
			{
				if (!_handlerHelper.HasRedLockKeyBeenAcquired(redLock.IsAcquired, resource, "Final validation", loanNumber)) return;

				var applicationCore = await TryGetApplicationCore(loanNumber);
				if (applicationCore == null)
				{
					_logger.LogError($"ApplicationCore not found for loanNumber: {loanNumber}");
					return;
				}

				if (!IsValidVersion(context.Message, applicationCore))
					return;

				await _finalValidationService.Process(context.Message, applicationCore);
			}

			timer.Stop();
			TimeSpan timeTaken = timer.Elapsed;
			_logger.LogInformation($"Total time FinalValidationHandler: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {loanNumber}");
		}

		private async Task<ApplicationCore> TryGetApplicationCore(string loanNumber)
		{
			var applicationCore = await _applicationCoreRepository.FindByLoanNumberAsync(loanNumber);

			if (applicationCore == null)
			{
				_logger.LogWarning($"ApplicationCore not found. LoanNumber: {loanNumber}");
			}

			return applicationCore;
		}

		private bool IsValidVersion(ProcessFinalValidation message, ApplicationCore applicationCore)
		{
			if (message.Version.HasValue)
			{
				if (message.Version != applicationCore.ProcessingVersion)
				{
					_logger.LogInformation($"Rejected. Old processing version. Loan Number: {message.LoanNumber}");
					return false;
				}
			}

			return true;
		}
	}
}