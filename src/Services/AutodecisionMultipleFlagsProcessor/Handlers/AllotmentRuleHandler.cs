using AutodecisionCore.Contracts.Messages;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Utility;
using AutodecisionCore.Contracts.Constants;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class AllotmentRuleHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<AllotmentRuleHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public AllotmentRuleHandler(ILogger<AllotmentRuleHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }
        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.AllotmentValidationOptionTwo, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.AllotmentValidationOptionTwo, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.PaymentType != "allotment" && autodecisionCompositeData.Application.PaymentType != "split_direct_deposit")
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = $"Application payment type is not Allotment/SDD.";
                    return response;
                }

                if (
                    autodecisionCompositeData.Application.Type == ApplicationType.Refi
                    && autodecisionCompositeData.Application.ReconciliationSystem == ReconciliationSystem.JpMorgan
                    && autodecisionCompositeData.Application.JpmExistsPending)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"Invalid JP Morgan Account.";
                    return response;
                }

                var lastBookedApplication = autodecisionCompositeData.LastApplications
                    .Where(x => x.Status == "6")
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                if (lastBookedApplication == null)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"First Loan. Need to request allotment.";
                    return response;
                }

                if (lastBookedApplication.AmountOfPayment != autodecisionCompositeData.Application.AmountOfPayment
                    || lastBookedApplication.ReconciliationSystem != autodecisionCompositeData.Application.ReconciliationSystem
                   )
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"Reconciliation system or Value has changed.";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                response.Message = $"Allotment or SDD didn't changed since last application.";
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.AllotmentValidationOptionTwo} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }

        }
    }
}
