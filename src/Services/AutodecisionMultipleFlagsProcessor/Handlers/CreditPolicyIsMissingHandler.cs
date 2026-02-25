using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class CreditPolicyIsMissingHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<CreditPolicyIsMissingHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public CreditPolicyIsMissingHandler(ILogger<CreditPolicyIsMissingHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.CreditPolicyIsMissing, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData data)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.CreditPolicyIsMissing, data, FlagResultEnum.Processed);
            try
            {
                if (data.CreditPolicy.EmployerRules.EmployerRulesItems.Count > 0) return response;
                _flagHelper.RaiseFlag(response, $"Credit Policy is missing for the Employer: {data.Application.EmployerName} in the State: {data.Application.StateAbbreviation}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: {FlagCode.CreditPolicyIsMissing} was not successfully processed for LoanNumber: {data.Application.LoanNumber} | Error: {ex.Message}");
                response.Message = ex.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
