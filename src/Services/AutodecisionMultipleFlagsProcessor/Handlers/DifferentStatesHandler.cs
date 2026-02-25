using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class DifferentStatesHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<DifferentStatesHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public DifferentStatesHandler(ILogger<DifferentStatesHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.DifferentStates, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {

            var response = _flagHelper.BuildFlagResponse(FlagCode.DifferentStates, autodecisionCompositeData);

			try
            {

                if (string.IsNullOrEmpty(autodecisionCompositeData.Application.StateAbbreviation))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.StateAbbreviation != autodecisionCompositeData.Application.LoanTermsStateAbbreviation)
                {
                    response.Message = $"Loan terms state: {autodecisionCompositeData.Application.LoanTermsStateAbbreviation} is different from application state: {autodecisionCompositeData.Application.StateAbbreviation}";
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    return response;
                }

                var allWarnings = FindAllApplicationWarningsByType(autodecisionCompositeData, ApplicationWarningType.NotLicensedState);

                if (allWarnings.Count > 0)
                {
                    response.Message = allWarnings[0].Description;
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.DifferentStates} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private List<ApplicationWarning> FindAllApplicationWarningsByType(AutodecisionCompositeData autodecisionCompositeData, string warningType)
        {
            var applicationWarningsList = new List<ApplicationWarning>();
            foreach (var warning in autodecisionCompositeData.ApplicationWarnings)
            {
                if (warning.Type == warningType)
                {
                    applicationWarningsList.Add(warning);
                }
            }
            return applicationWarningsList == null ? null : applicationWarningsList;
        }
    }
}