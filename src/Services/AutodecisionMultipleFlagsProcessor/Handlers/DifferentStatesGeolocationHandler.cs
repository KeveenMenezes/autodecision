using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{

    public class DifferentStatesGeolocationHandler : IConsumer<ProcessFlagRequestEvent>
    {

        private readonly ILogger<DifferentStatesGeolocationHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public DifferentStatesGeolocationHandler(ILogger<DifferentStatesGeolocationHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.DifferentStatesGeolocation, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.DifferentStatesGeolocation, autodecisionCompositeData);

			try 
            { 
                if (autodecisionCompositeData.Application.Type != ApplicationType.NewLoan)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                var stateAbbreviation = autodecisionCompositeData.Application.StateAbbreviation;
                var stateIpUserRequest = autodecisionCompositeData.Application.StateIpUserRequest;

                if (string.IsNullOrEmpty(stateAbbreviation) || string.IsNullOrEmpty(stateIpUserRequest))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (stateAbbreviation.ToUpper() != stateIpUserRequest.ToUpper())
                {
                    response.Message = $"Customer state address is {stateAbbreviation} and IP Geolocation state is {stateIpUserRequest}";
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.DifferentStatesGeolocation} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }

        }
    }
}