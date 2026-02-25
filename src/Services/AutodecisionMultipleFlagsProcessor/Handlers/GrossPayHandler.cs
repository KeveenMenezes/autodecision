using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class GrossPayHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<GrossPayHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public GrossPayHandler(
            ILogger<GrossPayHandler> logger,
            IFlagHelper flagHelper
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.GrossPay, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.GrossPay, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                if(!DefaultValidationsSucceeded(autodecisionCompositeData, response)) 
                    return response;

                var minRange = (1 - autodecisionCompositeData.Census.FlagGrossPayValue) * autodecisionCompositeData.Census.SalaryPerPeriod.Value;
                var maxRange = (1 + autodecisionCompositeData.Census.FlagGrossPayValue) * autodecisionCompositeData.Census.SalaryPerPeriod.Value;

                if (!autodecisionCompositeData.Customer.Salary.HasValue)
                {
                    _flagHelper.RaiseFlag(response, "Gross pay was not informed by the customer.");
                    return response;
                }

                var description = $"Gross Pay different from census. Application: {autodecisionCompositeData.Customer.Salary.Value.ToString("C")} | Census: {autodecisionCompositeData.Census.SalaryPerPeriod.Value.ToString("C")}";
                
                if (autodecisionCompositeData.Customer.Salary < minRange || autodecisionCompositeData.Customer.Salary > maxRange) _flagHelper.RaiseFlag(response, description);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.GrossPay} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (autodecisionCompositeData.Census?.FlagGrossPayValue == null)
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            if (autodecisionCompositeData.Census.CustomerId == 0)
            {
                _flagHelper.RaiseFlag(response, "Employee not found on census database.");
                return false;
            }
            if (autodecisionCompositeData.Census.FlagGrossPayValue == 0)
            {
                _flagHelper.RaiseFlag(response, "Gross Pay range value not found on census database.");
                return false;
            }
            if (!autodecisionCompositeData.Census.SalaryPerPeriod.HasValue || autodecisionCompositeData.Census.SalaryPerPeriod.Value == 0)
            {
                _flagHelper.RaiseFlag(response, "Salary not found on census.");
                return false;
            }
            return true;
        }
    }
}