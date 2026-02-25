using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class MandatoryHRVerificationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<MandatoryHRVerificationHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public MandatoryHRVerificationHandler(
            ILogger<MandatoryHRVerificationHandler> logger,
            IFlagHelper flagHelper
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.MandatoryHRVerification, _logger))
                return;


			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.MandatoryHRVerification, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response))
                    return response;

                _flagHelper.RaiseFlag(response, "Pending HR Verification");
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.MandatoryHRVerification} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (autodecisionCompositeData.Application.Program == BMGMoneyProgram.LoansAtWork && (!autodecisionCompositeData.Census?.CensusValidation).GetValueOrDefault(false)
                && (!autodecisionCompositeData.Census?.FlagReverseCensusActive).GetValueOrDefault(false))
            {
                return true; 
            }

            response.FlagResult = FlagResultEnum.Ignored;
            return false; 
        }
    }
}
