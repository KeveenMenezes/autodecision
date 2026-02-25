using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OpenPayrollSSNDoesNotMatchHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenPayrollSSNDoesNotMatchHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenPayrollSSNDoesNotMatchHandler(
            ILogger<OpenPayrollSSNDoesNotMatchHandler> logger,
            IFlagHelper flagHelper
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenPayrollSSNDoesNotMatch, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenPayrollSSNDoesNotMatch, autodecisionCompositeData, FlagResultEnum.Processed);

            try
            {
                if (autodecisionCompositeData.OpenPayroll.Connections is not null)
                {
                    if (!string.IsNullOrEmpty(autodecisionCompositeData.Customer.Ssn) && autodecisionCompositeData.OpenPayroll.Connections.Any(_ => !string.IsNullOrEmpty(_.ProfileInformation.SSN)))
                    {
                        var lastFourFromVendor = GetLastFour(autodecisionCompositeData.OpenPayroll.Connections.FirstOrDefault(_ => _.ProfileInformation.SSN is not null)?.ProfileInformation.SSN);

                        if (lastFourFromVendor.Contains("*"))
                        {
                            response.FlagResult = FlagResultEnum.Ignored;
                            return response;
                        }

                        if (lastFourFromVendor != GetLastFour(autodecisionCompositeData.Customer.Ssn))
                            _flagHelper.RaiseFlag(response, "The SSN informed in the open payroll does not match the customer's SSN");
                    }
                }
                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.OpenPayrollSSNDoesNotMatch} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private string GetLastFour(string source)
        {
            return source.Substring(source.Length - 4);
        }
    }
}
