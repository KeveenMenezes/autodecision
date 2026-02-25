using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class BrowserFringerprintHandler : IConsumer<ProcessFlagRequestEvent>
    {

        private readonly ILogger<BrowserFringerprintHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;
        public BrowserFringerprintHandler(ILogger<BrowserFringerprintHandler> logger, IFlagHelper flagHelper, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.BrowserFingerprint, _logger))
				return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.BrowserFingerprint, autodecisionCompositeData);

			try
            {
               var otherApplications = _customerInfo.GetOtherApplicationsWithSameFingerprint(autodecisionCompositeData.Application.BrowserFingerprint, autodecisionCompositeData.Customer.Id).Result;

                if (otherApplications.Any())
                {
                    response.Message = GetDescription(otherApplications, 10);
                    response.FlagResult = AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval;

                    return response;
                }

                response.FlagResult = AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.BrowserFingerprint} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetDescription(IEnumerable<ApplicationDto> applications, int count)
        {
            string description = $"Device previously detected | Same browser fingerprint found for other {applications.Count()} customer(s): ";
            if (applications.Count() > count)
                description += string.Join(",", applications.Take(count).Select(p => $"#{p.LoanNumber}")) + ", ...";
            else
                description += string.Join(",", applications.Select(p => $"#{p.LoanNumber}"));

            return description;
        }
    }
}
