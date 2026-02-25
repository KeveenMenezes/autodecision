using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class TransunionScoresHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<TransunionScoresHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;

        public TransunionScoresHandler(
            ILogger<TransunionScoresHandler> logger, 
            IFlagHelper flagHelper, 
            IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = featureToggleClient;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.TransunionScores, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.TransunionScores, autodecisionCompositeData);

            try
            {   

                if (_featureToggleClient.IsEnabled("new_autodecision_ignore_some_flags"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.TransunionResult == null)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }
                
                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi && autodecisionCompositeData.TransunionResult.OfacHit != "1")
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.TransunionResult.OfacHit == "1")
                {
                    _flagHelper.RaiseFlag(response, "OFAC found on the application");
                    return response;
                }

                var minValue = 99;

                List<string> reasons = new List<string>();

                if (autodecisionCompositeData.TransunionResult.Score < minValue)
                    reasons.Add($"Overall score: {autodecisionCompositeData.TransunionResult.Score}");

                if (autodecisionCompositeData.TransunionResult.DobScore < minValue)
                    reasons.Add($"Date of Birth score: {autodecisionCompositeData.TransunionResult.DobScore}");

                if (autodecisionCompositeData.TransunionResult.AddressScore < minValue)
                    reasons.Add($"Address score: {autodecisionCompositeData.TransunionResult.AddressScore}");

                if (autodecisionCompositeData.TransunionResult.NameScore < minValue)
                    reasons.Add($"Name score: {autodecisionCompositeData.TransunionResult.NameScore}");

                if (autodecisionCompositeData.TransunionResult.SSNScore < minValue)
                    reasons.Add($"SSN score: {autodecisionCompositeData.TransunionResult.SSNScore}");

                if (reasons.Count > 0)
                {
                    if (autodecisionCompositeData.Clarity is not null)
                    {
                        if (ValidateUsingClarity(autodecisionCompositeData))
                        {
                            response.FlagResult = FlagResultEnum.Approved;
                            response.ApprovalNote = "Clarity full match - (Match) Full DOB available and matched input (within +/- 1 year if month.day exact)\t(5) Exact match on first and last name; Exact match on address\t(5) Exact SSN match, Exact Name match, Exact Address match";
                            return response;
                        }
                    }

                    _flagHelper.RaiseFlag(response, $"TU Score - The customer has one or more scores that are below {minValue} - " + string.Join(", ", reasons));

                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.TransunionScores} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

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