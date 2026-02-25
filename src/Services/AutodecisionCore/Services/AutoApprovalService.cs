using AutodecisionCore.AutoApprovalCore.Interface;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories;
using AutodecisionCore.Services.Interfaces;
using System.Diagnostics;

namespace AutodecisionCore.Services
{
	public class AutoApprovalService : IAutoApprovalService
	{
		private readonly IAutoApprovalManager _autoApprovalManager;
		private readonly IAutoApprovalFundingMethodRepository _autoApprovalFundingMethodRepository;
		private readonly IAutoApprovalPaymentTypeRepository _autoApprovalPaymentTypeRepository;
		private readonly IAutoApprovalUwClusterRepository _autoApprovalUwClusterRepository;
		private readonly ILogger<AutoApprovalService> _logger;

		public AutoApprovalService(
			IAutoApprovalManager autoApprovalManager, 
			IAutoApprovalPaymentTypeRepository autoApprovalPaymentTypeRepository,
			IAutoApprovalFundingMethodRepository autoApprovalFundingMethodRepository,
			IAutoApprovalUwClusterRepository autoApprovalUwClusterRepository,
			ILogger<AutoApprovalService> logger)
		{
			_autoApprovalManager = autoApprovalManager;
			_autoApprovalPaymentTypeRepository = autoApprovalPaymentTypeRepository;
			_autoApprovalFundingMethodRepository = autoApprovalFundingMethodRepository;
			_autoApprovalUwClusterRepository = autoApprovalUwClusterRepository;
			_logger = logger;
		}

		public async Task RunAutoApproval(ApplicationCore applicationCore, AutodecisionCompositeData autodecisionCompositeData, string loanNumber, string reason)
		{
			var timer = new Stopwatch();
			timer.Start();

			var autoApprovalRequest = await FillAutoApprovalRequest(autodecisionCompositeData, applicationCore,loanNumber, reason);

			timer.Stop();
			var timeTaken = timer.Elapsed;
			_logger.LogInformation($"Time to FillAutoApprovalRequest: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {loanNumber}");
			timer.Restart();

			await _autoApprovalManager.RunRule(autoApprovalRequest, applicationCore);

			timer.Stop();
			timeTaken = timer.Elapsed;
			_logger.LogInformation($"Time to RunRule: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {loanNumber}");
		}

		#region private methods
		private async Task<AutoApprovalRequest> FillAutoApprovalRequest(AutodecisionCompositeData autodecisionCompositeData, ApplicationCore applicationCore, string loanNumber, string reason)
		{
			AutoApprovalRequest autoApprovalRequest = new AutoApprovalRequest()
			{
				AutoApprovalFundingMethods = await _autoApprovalFundingMethodRepository.GetAll(),
				AutoApprovalPaymentTypes = await _autoApprovalPaymentTypeRepository.GetAll(),
				AutoApprovalUwClusters = await _autoApprovalUwClusterRepository.GetAll(),
				Application = autodecisionCompositeData.Application,
				LastApplications = autodecisionCompositeData.LastApplications,
				LoanNumber = loanNumber,
				OpenBanking = autodecisionCompositeData.OpenBanking,
				OpenPayroll = autodecisionCompositeData.OpenPayroll,
				CreditPolicy = autodecisionCompositeData.CreditPolicy,
				Customer = autodecisionCompositeData.Customer,
				DebitCard = autodecisionCompositeData.DebitCard,
				FaceRecognition = autodecisionCompositeData.FaceRecognition,
				AllotmentRequested = applicationCore.HasAllotmentNeeded(),
				CreditRisk = autodecisionCompositeData.CreditRisk,
                Reason = reason,
				Employer = autodecisionCompositeData.Employer,
				ApplicationDocuments =  autodecisionCompositeData.FlagValidatorHelper.ApplicationDocuments
            };

			return autoApprovalRequest;
		}
		#endregion
	}
}
