using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class DebitCardBankAccountAnalysisHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<DebitCardBankAccountAnalysisHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public DebitCardBankAccountAnalysisHandler(
            ILogger<DebitCardBankAccountAnalysisHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.DebitCardBankAccountAnalysis, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.DebitCardBankAccountAnalysis, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                var shouldConnectOpenBanking = OpenConnectionsHelper.ShouldConnectOpenBanking(autodecisionCompositeData.CreditPolicy.EmployerRules.EmployerRulesItems);
                var shouldConnectOpenPayroll = OpenConnectionsHelper.ShouldConnectOpenPayroll(autodecisionCompositeData.CreditPolicy.EmployerRules.EmployerRulesItems);

                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response, shouldConnectOpenBanking, shouldConnectOpenPayroll)) return response;

                var validOpenBankingAccount = _flagHelper.ValidOpenBankingAccount(autodecisionCompositeData, response, shouldConnectOpenBanking);
                if (!validOpenBankingAccount.isSuccess) return validOpenBankingAccount.response;

                var validOpenPayrollRemainderAccount = _flagHelper.ValidOpenPayrollRemainderAccount(autodecisionCompositeData, response, shouldConnectOpenPayroll);
                if (!validOpenPayrollRemainderAccount.isSuccess) return validOpenPayrollRemainderAccount.response;

                if (!CheckSameDebitCardAsLastApplication(autodecisionCompositeData, response)) return response;

                if (!ValidDebitCardBinEmissor(autodecisionCompositeData, response)) return response;

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.DebitCardBankAccountAnalysis} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private bool DefaultValidationsSucceeded(AutodecisionCompositeData compositeData, ProcessFlagResponseEvent response, bool shouldConnectOpenBanking, bool shouldConnectOpenPayroll)
        {
            if (compositeData.Application.FundingMethod != ApplicationFundingMethod.DebitCard && compositeData.Application.PaymentType != PayrollType.DebitCard)
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            if (!OpenConnectionsHelper.HasDebitCardConnection(compositeData.DebitCard))
            {
                _flagHelper.RaiseFlag(response, "Debit Card not connected!");
                return false;
            }
            if (compositeData.DebitCard.IsConnected && !shouldConnectOpenBanking && !shouldConnectOpenPayroll)
            {
                return true;
            }
            return true;
        }

        private bool ValidDebitCardBinEmissor(AutodecisionCompositeData compositeData, ProcessFlagResponseEvent response)
        {
            if (string.IsNullOrEmpty(compositeData.DebitCard.CardBinEmissor))
            {
                _flagHelper.RaiseFlag(response, "Debit Card issuer not found to compare with Bank Name");
                return false;
            }
            if (!compositeData.DebitCard.IsConnected && compositeData.DebitCard.BinName != compositeData.Customer.BankName)
            {
                _flagHelper.RaiseFlag(response, $"Debit card issuer {compositeData.DebitCard.BinName} is not related to ${compositeData.Customer.BankName}");
                return false;
            }
            return true;
        }

        private bool CheckSameDebitCardAsLastApplication(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            var lastBookedApplication = autodecisionCompositeData.LastApplications?
                    .Where(x => x.Status == "6")
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

            if (lastBookedApplication == null || lastBookedApplication.ApplicationConnections == null)
                return true;

            if (autodecisionCompositeData.DebitCard.Id == null || autodecisionCompositeData.DebitCard.Id == 0)
                return true;

            if (autodecisionCompositeData.DebitCard.Id == lastBookedApplication.ApplicationConnections.DebitCardId)
            {
                response.FlagResult = FlagResultEnum.Approved;
                response.ApprovalNote = $"Debit card remained the same as the previous loan";
                return false;
            }

            return true;
        }
    }
}