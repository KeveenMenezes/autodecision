using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OpenBankingNotConnectedHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenBankingNotConnectedHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenBankingNotConnectedHandler(
            ILogger<OpenBankingNotConnectedHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenBankingNotConnected, _logger)) return;
            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData compositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenBankingNotConnected, compositeData, FlagResultEnum.Processed);
            try
            {
                var shouldConnectOpenBanking = OpenConnectionsHelper.ShouldConnectOpenBanking(compositeData.CreditPolicy.EmployerRules.EmployerRulesItems);
                if (!shouldConnectOpenBanking)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                var openBankingConnections = compositeData?.OpenBanking?.Connections;
                var description = "Open Banking not connected";

                if (openBankingConnections is not null && openBankingConnections.Any())
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    return response;
                }

                if (VerifyBankStatementDocumentUpload(compositeData))
                {
                    response.Message = "There is bank statement uploaded and approved";
                    return response;
                }

                _flagHelper.RaiseFlag(response, description);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.OpenBankingNotConnected} was not successfully processed for LoanNumber: {compositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
        private static bool VerifyBankStatementDocumentUpload(AutodecisionCompositeData compositeData)
        {
            var hasBankStatementUploaded = compositeData.FlagValidatorHelper.ApplicationDocuments
                .FirstOrDefault(x => x.DocumentType == DocumentType.BankStatement && x.Uploaded && x.ReviewStatus == DocumentReviewStatus.Approved);

            return hasBankStatementUploaded is not null;
        }
    }
}