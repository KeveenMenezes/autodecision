using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class NoTUResponseHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<NoTUResponseHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public NoTUResponseHandler(ILogger<NoTUResponseHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.NoTUResponse, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.NoTUResponse, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.TransunionResult == null || autodecisionCompositeData.TransunionResult.SessionId == "-1")
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "No response from TU";

                    if (autodecisionCompositeData.Clarity is not null)
                    {
                        if (ValidateUsingClarity(autodecisionCompositeData))
                        {
                            response.FlagResult = FlagResultEnum.Approved;
                            response.ApprovalNote = "Clarity full match - (Match) Full DOB available and matched input (within +/- 1 year if month.day exact)\t(5) Exact match on first and last name; Exact match on address\t(5) Exact SSN match, Exact Name match, Exact Address match";
                        }
                    }

                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.NoTUResponse} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        public bool ValidateUsingClarity(AutodecisionCompositeData autodecisionCompositeData)
        {
            if
            (
                autodecisionCompositeData.Clarity.DobMatchResult == "Match" &&
                autodecisionCompositeData.Clarity.DobMatchDescription == "Full DOB available and matched input (within +/- 1 year if month.day exact)" &&
                autodecisionCompositeData.Clarity.NameAddressMatchConfidence == 5 &&
                autodecisionCompositeData.Clarity.NameAddressMatchDescription == "Exact match on first and last name; Exact match on address" &&
                autodecisionCompositeData.Clarity.SSNNameAddressMatchConfidence == 5 &&
                autodecisionCompositeData.Clarity.SSNNameAddressMatchDescription == "Exact SSN match, Exact Name match, Exact Address match"
            ) 
            {
                return true;
            }
            return false;
        }

    }
}