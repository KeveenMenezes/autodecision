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
    public class OpenPayrollOcrolusHireDateHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenPayrollOcrolusHireDateHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenPayrollOcrolusHireDateHandler(
            ILogger<OpenPayrollOcrolusHireDateHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenPayrollOcrolusHireDate, _logger))
            {
                return;
            }

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenPayrollOcrolusHireDate, autodecisionCompositeData, FlagResultEnum.Ignored);
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

                if (mostRecentlyActiveConnection.HireDate is null)
                {
                    _flagHelper.RaiseFlag(response, "Ocrolus hire date is not available! Please verify the document manually!");
                    return response;
                }

                _flagHelper.RaiseFlag(response, "Ocrolus hire date is available! Please verify the document manually!");
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FlagCode: [{FlagCode}] was not successfully processed for LoanNumber: {LoanNumber} | Error: {ExMessage}", FlagCode.OpenPayrollOcrolusHireDate, autodecisionCompositeData.Application.LoanNumber, e.Message);

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }


    }
}