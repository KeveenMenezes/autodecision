using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class EligibilityRuleHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<EligibilityRuleHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public EligibilityRuleHandler(
            ILogger<EligibilityRuleHandler> logger,
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
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.EligibilityRule, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.EligibilityRule, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response))
                    return response;

                var criteria = autodecisionCompositeData.Census.FlagEligibilityRuleValue;
                var censusRecord = _customerInfo.GetCensusByCustomerIdWithCriteria(autodecisionCompositeData.Application.EmployerId, autodecisionCompositeData.Application.CustomerId, criteria).Result;

                if (censusRecord is null || censusRecord.TransactionStatus == CensusTransactionStatus.Error)
                {
                    var message = "Error when searching Census by Criteria. Please review the Census Criteria.";
                    _logger.LogError($"FlagCode: {FlagCode.EligibilityRule} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | {message}");

                    response.Message = message;
                    response.FlagResult = FlagResultEnum.Error;
                    return response;
                }

                if (censusRecord.TransactionStatus is CensusTransactionStatus.NotFound)
                {
                    _flagHelper.RaiseFlag(response, "Please check census: invalid group, status code or inactive.");

                    if (autodecisionCompositeData.Application.ProductId == ApplicationProductId.Cashless)
                        return response;

                    if (autodecisionCompositeData.WhiteList is null)
                    {
                        response.FlagResult = FlagResultEnum.AutoDeny;
                        response.Message = "Application Denied due to customer doesn't meet the census criteria.";
                        return response;
                    }
                    return response;
                }

                if (autodecisionCompositeData.Census.EmployerId == 48 && autodecisionCompositeData.Census.TimeType == "Probationary") // City of miami and probationary
                    _flagHelper.RaiseFlag(response, "Customer probationary status on City of Miami. Please approve or decline manually.");
                else if (autodecisionCompositeData.Census.EmployerId == 124 && autodecisionCompositeData.Census.PayrollGroup == "AB") //  Miami-Dade County and probationary
                    _flagHelper.RaiseFlag(response, "Customer probationary status on Miami-Dade County. Please approve or decline manually.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.EligibilityRule} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (string.IsNullOrEmpty(autodecisionCompositeData.Census?.FlagEligibilityRuleValue))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            if (autodecisionCompositeData.Census.CustomerId == 0)
            {
                _flagHelper.RaiseFlag(response, "Customer not found on census. Employment verification required.");
                return false;
            }
            return true;
        }
    }
}