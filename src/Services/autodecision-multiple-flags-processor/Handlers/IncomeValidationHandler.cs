using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class IncomeValidationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<IncomeValidationHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;

        public IncomeValidationHandler(
            ILogger<IncomeValidationHandler> logger,
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

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.IncomeValidation, _logger))
            {
                return;
            }

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionComposite)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.IncomeValidation, autodecisionComposite, FlagResultEnum.Ignored);

            try
            {
                if (!_featureToggleClient.IsEnabled("FeatureLineAssignment"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = "Flag not released.";
                    return response;
                }

                bool isLAW = FlagValidatorHelper.IsLAWProgram(autodecisionComposite.Application.Program);

                return isLAW 
                    ? VerifyTotalIncomeLAW(autodecisionComposite, response) 
                    :VerifyTotalIncomeLFFOrLFA(autodecisionComposite, response);               
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FlagCode: [{FlagCode}] was not successfully processed for LoanNumber: {LoanNumber} | Error: {ExMessage}", FlagCode.IncomeValidation, autodecisionComposite.Application.LoanNumber, e.Message);

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static ProcessFlagResponseEvent VerifyTotalIncomeLAW(AutodecisionCompositeData autodecisionComposite, ProcessFlagResponseEvent response)
        {           
            var mainIncome = autodecisionComposite.TotalIncome.Incomes.FirstOrDefault(s=> s.TypeIncome == (int)TypeIncome.Main);
            if (mainIncome != null)
            {
                if (mainIncome.Status == StatusIncome.Approved)
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    response.Message = "Main income verified";
                    return response;
                }
                if (mainIncome.Status == StatusIncome.Pending)
                {
                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = "Main income is pending verification";
                    return response;
                }               
            }
            else
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message = "No main income found.";
            }
            return response;
        }

        private static ProcessFlagResponseEvent VerifyTotalIncomeLFFOrLFA(AutodecisionCompositeData autodecisionComposite, ProcessFlagResponseEvent response)
        {          
            var totalIncome = autodecisionComposite.TotalIncome;
            if (totalIncome.Status == StatusIncome.Reproved)
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message = "Income not approved due to discrepancies found during document analysis.";
                return response;
            }
            if (totalIncome.Status == StatusIncome.Pending)
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message = "There are pending income verifications.";
                return response;
            }

            response.FlagResult = FlagResultEnum.Processed;
            response.Message = "Income proven by the customer";

            return response;
        }
    }
}
