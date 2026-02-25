using AutodecisionCore.Contracts.Constants;
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
    public class InternetBankHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<InternetBankHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;

        public InternetBankHandler(ILogger<InternetBankHandler> logger, IFlagHelper flagHelper, IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = featureToggleClient;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.InternetBank, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.InternetBank, autodecisionCompositeData);

            try
            {
                if (autodecisionCompositeData.Application.FundingMethod == ApplicationFundingMethod.Check)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.ProductId == ApplicationProductId.ProductAtWork)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.HighRisk)
                {
                    var sameBankInfoLastApplication = SameBankInfoLastApplication(autodecisionCompositeData);
                    if (sameBankInfoLastApplication != null)
                    {
                        response.FlagResult = FlagResultEnum.Approved;
                        response.ApprovalNote = $"Customer has the same bank information as the previous application. Prev. loan number {sameBankInfoLastApplication.LoanNumber} | Routing number {sameBankInfoLastApplication.BankRoutingNumber} |Bank acc.  {sameBankInfoLastApplication.BankRoutingNumber}.";
                        return response;
                    }

                    var openBankingConnections = autodecisionCompositeData?.OpenBanking?.Connections;

                    if (openBankingConnections is not null && openBankingConnections.Any())
                    {
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }

                    response.FlagResult = FlagResultEnum.PendingApproval;
                    response.Message = $"Bank is flagged as \"High Risk Bank\" - Name: {autodecisionCompositeData.Customer.BankName} - Routing number: {autodecisionCompositeData.Application.BankRoutingNumber}";
                    return response;
                }

                response.FlagResult = FlagResultEnum.Processed;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.InternetBank} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private LastApplication? SameBankInfoLastApplication(AutodecisionCompositeData autodecisionCompositeData)
        {
            var previousApplicationId = autodecisionCompositeData.Application.PreviousApplicationId;
            if (previousApplicationId != null)
            {
                var lastApplication = autodecisionCompositeData.LastApplications.FirstOrDefault(application => application.Id.Equals(previousApplicationId));
                if (lastApplication != null)
                {
                    if (string.Equals(autodecisionCompositeData.Application.BankAccountNumber, lastApplication.BankAccountNumber) &&
                    string.Equals(autodecisionCompositeData.Application.BankRoutingNumber, lastApplication.BankRoutingNumber))
                    {
                        return lastApplication;
                    }
                }
            }
            return null;
        }

    }
}