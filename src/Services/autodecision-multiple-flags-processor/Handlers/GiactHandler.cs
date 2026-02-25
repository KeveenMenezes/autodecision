using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class GiactHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<GiactHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ICustomerInfo _customerInfo;

        public GiactHandler(ILogger<GiactHandler> logger, IFlagHelper flagHelper, IFeatureToggleClient featureToggleClient, ICustomerInfo customerInfo)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = featureToggleClient;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.Giact, _logger))
                return;

            var response = await ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;

            await _flagHelper.SendReponseMessage(response);
        }

        public async Task<ProcessFlagResponseEvent> ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.Giact, autodecisionCompositeData);
            var shouldConnectOpenBanking = OpenConnectionsHelper.ShouldConnectOpenBanking(autodecisionCompositeData.CreditPolicy.EmployerRules.EmployerRulesItems);
            var shouldConnectOpenPayroll = OpenConnectionsHelper.ShouldConnectOpenPayroll(autodecisionCompositeData.CreditPolicy.EmployerRules.EmployerRulesItems);

            try
            {
                var validOpenBankingAccount = _flagHelper.ValidOpenBankingAccount(autodecisionCompositeData, response, shouldConnectOpenBanking);
                var validOpenPayrollRemainderAccount = _flagHelper.ValidOpenPayrollRemainderAccount(autodecisionCompositeData, response, shouldConnectOpenPayroll);

                if (autodecisionCompositeData.Application.Type == ApplicationType.NewLoan && autodecisionCompositeData.Application.Program == ApplicationProgram.LoansAtWork)
                {
                    if (!validOpenBankingAccount.isSuccess)
                    {
                        _flagHelper.RaiseFlag(response, "Payroll deposit cannot be verified");
                        return response;
                    }
                }

                if (validOpenBankingAccount.isSuccess && validOpenPayrollRemainderAccount.isSuccess)
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    return response;
                }

                if (autodecisionCompositeData.Application.ProductId == ApplicationProductId.Cashless ||
                    autodecisionCompositeData.Application.ProductId == ApplicationProductId.CashlessAndDeferredPayment)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.FundingMethod == ApplicationFundingMethod.Check && autodecisionCompositeData.Application.Type == ApplicationType.NewLoan)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.Program != ApplicationProgram.LoansAtWork && _featureToggleClient.IsEnabled("giact_run_manually"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.Type == ApplicationType.Refi && _featureToggleClient.IsEnabled("bypass_giact_for_refi"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                if (autodecisionCompositeData.Application.FundingMethod == ApplicationFundingMethod.Check && autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                {
                    _flagHelper.RaiseFlag(response, "Funding Method Check - Refi App");
                    return response;
                }

                var giact_result = await _customerInfo.GetLastGiactResult(autodecisionCompositeData.Application.LoanNumber);
                if (giact_result == null)
                {
                    _flagHelper.RaiseFlag(response, "No GIACT result found");
                    return response;
                }
                else
                {
                    string account_response_status = string.IsNullOrEmpty(giact_result.account_response_status) ? string.Empty : giact_result.account_response_status.ToLower().Trim();
                    string customer_response_status = string.IsNullOrEmpty(giact_result.customer_response_status) ? string.Empty : giact_result.customer_response_status.ToLower().Trim();
                    string account_response_code = string.IsNullOrEmpty(giact_result.account_response_code) ? string.Empty : giact_result.account_response_code.ToLower().Trim();

                    if (account_response_status == "pass" && customer_response_status == "pass")
                    {
                        response.FlagResult = FlagResultEnum.Processed;
                        return response;
                    }

                    _flagHelper.RaiseFlag(response, $"GIACT returned a risk result: {giact_result.verification_response}. Please check it.");

                    if (autodecisionCompositeData.Application.PreviousApplicationId != null && account_response_status != "declined" && autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                    {
                        LastApplication PreviousApplication = autodecisionCompositeData.LastApplications != null ? autodecisionCompositeData.LastApplications.FirstOrDefault(x => x.Id == autodecisionCompositeData.Application.PreviousApplicationId.Value) : null;
                        if (PreviousApplication != null)
                        {
                            if (string.Equals(PreviousApplication.BankRoutingNumber, autodecisionCompositeData.Application.BankRoutingNumber) && string.Equals(PreviousApplication.BankAccountNumber, autodecisionCompositeData.Application.BankAccountNumber))
                            {
                                response.FlagResult = FlagResultEnum.Approved;
                                response.ApprovalNote = $"Customer has the same bank information as the previous application. Prev. loan number {PreviousApplication.LoanNumber} | Routing number {PreviousApplication.BankRoutingNumber} |Bank acc.  {PreviousApplication.BankAccountNumber}.";
                                return response;
                            }
                        }
                    }

                    return response;
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.Giact} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }
    }
}
