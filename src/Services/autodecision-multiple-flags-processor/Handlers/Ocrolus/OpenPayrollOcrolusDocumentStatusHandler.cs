using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers.Ocrolus
{
    public class OpenPayrollOcrolusDocumentStatusHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenPayrollOcrolusDocumentStatusHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenPayrollOcrolusDocumentStatusHandler(
            ILogger<OpenPayrollOcrolusDocumentStatusHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenPayrollOcrolusDocumentStatus, _logger))
            {
                return;
            }

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenPayrollOcrolusDocumentStatus, autodecisionCompositeData, FlagResultEnum.Ignored);
            try
            {
                if (autodecisionCompositeData.OpenPayroll == null)
                {
                    return response;
                }

                var mostRecentlyActiveConnection = autodecisionCompositeData.OpenPayroll.Connections
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.ConnectedAt)
                    .FirstOrDefault();

                if (mostRecentlyActiveConnection == null)
                {
                    return response;
                }

                if (mostRecentlyActiveConnection.VendorType != OpenPayrollVendorConstant.Ocrolus)
                {
                    return response;
                }

                if (mostRecentlyActiveConnection.OcrolusDocumentStatus is null)
                {
                    _flagHelper.RaiseFlag(response, "Ocrolus document status is not available at the moment!");
                    return response;
                }

                if (mostRecentlyActiveConnection.OcrolusDocumentStatus != OcrolusDocumentStatus.Verified)
                {
                    _flagHelper.RaiseFlag(response, $"Ocrolus document status is: [{OcrolusStatusMessageHelper(mostRecentlyActiveConnection.OcrolusDocumentStatus.Value)}]");
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FlagCode: [{FlagCode}] was not successfully processed for LoanNumber: {LoanNumber} | Error: {ExMessage}", FlagCode.OpenPayrollOcrolusDocumentStatus, autodecisionCompositeData.Application.LoanNumber, e.Message);

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static string OcrolusStatusMessageHelper (int ocrolusDocumentStatus)
        {
            switch (ocrolusDocumentStatus)
            {
                case OcrolusDocumentStatus.Pending:
                    return nameof(OcrolusDocumentStatus.Pending);
                case OcrolusDocumentStatus.Reject:
                    return nameof(OcrolusDocumentStatus.Reject);
                case OcrolusDocumentStatus.SignalFound:
                    return nameof(OcrolusDocumentStatus.SignalFound);
                case OcrolusDocumentStatus.Verified:
                    return nameof(OcrolusDocumentStatus.Verified);
                default:
                    return "Undefined status";
            }
        }
    }
}