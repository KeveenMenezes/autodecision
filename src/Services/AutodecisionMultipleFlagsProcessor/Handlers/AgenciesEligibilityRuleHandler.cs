using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class AgenciesEligibilityRuleHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<AgenciesEligibilityRuleHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public AgenciesEligibilityRuleHandler(
            ILogger<AgenciesEligibilityRuleHandler> logger,
            IFlagHelper flagHelper,
            ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.AgenciesEligibilityRule, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.AgenciesEligibilityRule, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response))
                    return response;

                var criteria = autodecisionCompositeData.Census.FlagAgenciesEligibilityRuleValue;
                var censusRecord = _customerInfo.GetCensusByCustomerIdWithCriteria(autodecisionCompositeData.Application.EmployerId, autodecisionCompositeData.Application.CustomerId, criteria).Result;

                if (censusRecord is null || censusRecord.TransactionStatus is CensusTransactionStatus.Error)
                {
                    var message = "Error when searching Census by Criteria. Please review the Census Criteria.";
                    _logger.LogError($"FlagCode: {FlagCode.AgenciesEligibilityRule} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | {message}");

                    response.Message = message;
                    response.FlagResult = FlagResultEnum.Error;
                    return response;
                }
                if (censusRecord.TransactionStatus is CensusTransactionStatus.NotFound)
                {
                    _flagHelper.RaiseFlag(response, "Please check census: invalid group, status code or inactive.");

                    if (autodecisionCompositeData.WhiteList is null)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Application Denied due to customer doesn't meet the census criteria.";
                        return response;
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.AgenciesEligibilityRule} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (string.IsNullOrEmpty(autodecisionCompositeData.Census?.FlagAgenciesEligibilityRuleValue))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            if (autodecisionCompositeData.Census?.CustomerId == 0)
            {
                _flagHelper.RaiseFlag(response, "Customer not found on census. Employment verification required.");
                return false;
            }
            return true;
        }
    }
}