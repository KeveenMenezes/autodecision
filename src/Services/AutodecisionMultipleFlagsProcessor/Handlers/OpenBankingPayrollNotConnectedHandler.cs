using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class OpenBankingPayrollNotConnectedHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenBankingPayrollNotConnectedHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenBankingPayrollNotConnectedHandler(
            ILogger<OpenBankingPayrollNotConnectedHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenBankingOrPayrollNotConnected, _logger)) return;
            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenBankingOrPayrollNotConnected, autodecisionCompositeData, FlagResultEnum.Processed);
            try
            {
                const int maxConnectedDaysAccepted = 15;
                var shouldConnect = OpenConnectionsHelper.MustConnectAtLeastOpenPayrollOrBanking(autodecisionCompositeData.CreditPolicy.EmployerRules.EmployerRulesItems);
                if (!shouldConnect)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                _flagHelper.RaiseFlag(response, "Open Banking or Open Payroll Required.");

                var isNewOpenPayrollApi = autodecisionCompositeData.OpenPayroll?.Connections.Any(x => x.IsNewOpenPayroll);
                var isOpenPayrollConnectionValid = false;

                if (isNewOpenPayrollApi.GetValueOrDefault())
                {
                    var isActiveConnection = autodecisionCompositeData.OpenPayroll?.Connections.FirstOrDefault()?.IsActive;
                    isOpenPayrollConnectionValid = isActiveConnection.GetValueOrDefault();

                    if (isOpenPayrollConnectionValid)
                    {
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }
                }
                else
                {
                    int? daysConnectedWithOpenPayroll = OpenConnectionsHelper.GetDaysFromLatestOpenPayrollConnection(autodecisionCompositeData.OpenPayroll?.Connections);
                    isOpenPayrollConnectionValid = daysConnectedWithOpenPayroll is not null and >= 0 and <= maxConnectedDaysAccepted;

                    if (isOpenPayrollConnectionValid)
                    {
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }
                }

                var openBankingConnections = autodecisionCompositeData?.OpenBanking?.Connections;

                if (openBankingConnections is not null && openBankingConnections.Any())
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    return response;
                }

                if (VerifyPaystubDocumentUpload(autodecisionCompositeData))
                {
                    response.Message = "There is paystub document uploaded and approved";
                    return response;
                }

                if (!isOpenPayrollConnectionValid)
                    _flagHelper.RaiseFlag(response, "Open Banking and Open Payroll is not connected");
                else
                    _flagHelper.RaiseFlag(response, "Invalid OpenPayroll, please check if it is an old connection or if it exists.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.OpenBankingOrPayrollNotConnected} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
        private static bool VerifyPaystubDocumentUpload(AutodecisionCompositeData autodecisionCompositeData)
        {
            var hasPaystubUploaded = autodecisionCompositeData.FlagValidatorHelper.ApplicationDocuments
                .FirstOrDefault(x => x.DocumentType == DocumentType.Paystub && x.Uploaded && x.ReviewStatus == DocumentReviewStatus.Approved);

            return hasPaystubUploaded is not null;
        }
    }
}