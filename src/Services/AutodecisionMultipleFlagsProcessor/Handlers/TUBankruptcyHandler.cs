using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Utility;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;
using Newtonsoft.Json.Linq;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class TUBankruptcyHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<TUBankruptcyHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        public readonly ICreditInquiry _creditInquiry;
        public readonly ICustomerInfo _customerInfo;
        private IFeatureToggleClient _featureToggleClient;

        private bool _successPacer;
        private bool _bankruptcyPacer;
        private bool _successTransunion;
        private bool _bankruptcyTransunion;

        public TUBankruptcyHandler(ILogger<TUBankruptcyHandler> logger, IFlagHelper flagHelper, ICreditInquiry creditInquiry, ICustomerInfo customerInfo, IFeatureToggleClient feature)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = feature;
            _creditInquiry = creditInquiry;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.TUBankruptcy, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.TUBankruptcy, autodecisionCompositeData);

            try
            {
                if (_featureToggleClient.IsEnabled("new_autodecision_ignore_some_flags"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                StartCustomerTransunionProcess(autodecisionCompositeData.Customer, autodecisionCompositeData.TransunionResult, autodecisionCompositeData.Application.LoanNumber);

                CheckPacerBankruptcy(autodecisionCompositeData.Application);

                if (!_successPacer || _featureToggleClient.IsDisabled("check_bk_pacer_first"))
                    CheckTransunionBankruptcy(autodecisionCompositeData.Application);
                ValidateProcesses(autodecisionCompositeData.Application, response);


                return response;

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagCode.TUBankruptcy} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private void StartCustomerTransunionProcess(Customer customer, TransunionResult transunion, string loanNumber)
        {
            if (_featureToggleClient.IsEnabled("autodecision_run_customer_transunion_process"))
            {
                if (_featureToggleClient.IsDisabled("clarity_identity_priority"))
                {
                    _logger.LogInformation(@$"193 Loan: {loanNumber} - Starting Customer Transunion Process");
                    if (transunion != null && !HasChangedInformation(customer, transunion) && transunion.SessionId != "clarity")
                    {
                        _creditInquiry.GetIdVerification(customer.Id);
                    }

                    _logger.LogInformation(@$"193 Loan: {loanNumber} - Customer Transunion Process Ended");
                }
            }
        }

        private bool HasChangedInformation(Customer customer, TransunionResult transunion)
        {
            if (Util.isFieldsNotEqual(customer.FirstName, transunion.FirstName))
                return true;

            if (Util.isFieldsNotEqual(customer.LastName, transunion.LastName))
                return true;

            if (Util.isFieldsNotEqual(customer.DateOfBirth, transunion.BirthDate))
                return true;

            if (Util.isFieldsNotEqual(customer.StreetAddress, transunion.StreetAddress))
                return true;

            if (Util.isFieldsNotEqual(customer.CityName, transunion.CityName))
                return true;

            if (Util.isFieldsNotEqual(customer.StateName, transunion.StateName))
                return true;

            if (Util.isFieldsNotEqual(customer.ZipCode, transunion.ZipCode))
                return true;

            string customerSsn = Cryptography.GenerateSHA512String(customer.Ssn);

            if (Util.isFieldsNotEqual(customerSsn, transunion.SSN))
                return true;

            return false;
        }

        private void CheckPacerBankruptcy(Application application)
        {
            if (_featureToggleClient.IsEnabled("autodecision_run_pacer"))
            {
                _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - Starting Pacer Process");

                var jsonResult = _creditInquiry.CheckPacerBankruptcy(application.LoanNumber);

                var result = JObject.Parse(jsonResult);

                _successPacer = Util.GetValueFromJSON<bool>(result["success"]);
                _bankruptcyPacer = Util.GetValueFromJSON<bool>(result["bankruptcy"]);

                _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - Pacer Process Ended");
            }
        }
        private void CheckTransunionBankruptcy(Application application)
        {
            if (_featureToggleClient.IsEnabled("autodecision_run_transunion"))
            {
                _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - Starting TU Process");

                var jsonResult = _creditInquiry.CheckTransunionBankruptcy(application.LoanNumber);

                var result = JObject.Parse(jsonResult);

                _successTransunion = Util.GetValueFromJSON<bool>(result["success"]);
                _bankruptcyTransunion = Util.GetValueFromJSON<bool>(result["bankruptcy"]);

                _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - TU Process Ended");
            }
        }

        private void ValidateProcesses(Application application, ProcessFlagResponseEvent response)
        {
            var pacerHasBankruptcy = _successPacer && _bankruptcyPacer;
            var transunionHasBankruptcy = _successTransunion && _bankruptcyTransunion;
            response.FlagResult = FlagResultEnum.Processed;

            if (pacerHasBankruptcy || transunionHasBankruptcy)
            {
                var description = $"Bankruptcy record found on {GetProcessHasBankruptcy(pacerHasBankruptcy, transunionHasBankruptcy)}";

                _flagHelper.RaiseFlag(response, description);

                if (pacerHasBankruptcy)
                {
                    _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - Pacer has BK - start");

                    var pacerValidationResults = _customerInfo.ValidateBankruptcy(application.Id);

                    if (!pacerValidationResults.CheckDeclineApp && pacerValidationResults.CheckApproveFlag)
                    {
                        response.ApprovalNote = "Pacer was able to validate BK information";
                        response.FlagResult = FlagResultEnum.Approved;
                    }

                    _logger.LogInformation(@$"193 Loan: {application.LoanNumber} - Pacer has BK - END");
                }
            }

            if (transunionHasBankruptcy && !_successPacer)
                _flagHelper.RaiseFlag(response, "Unable to run Pacer - Please verify it manually");

            if (!_successTransunion && !_successPacer)
                _flagHelper.RaiseFlag(response, "Unable to run TU and Pacer - Please verify it manually");

        }

        private string GetProcessHasBankruptcy(bool pacerHasBankruptcy, bool transunionHasBankruptcy)
        {
            if (pacerHasBankruptcy && transunionHasBankruptcy)
                return "Pacer & Transunion";

            if (pacerHasBankruptcy)
                return "Pacer";

            return "Transunion";
        }

    }
}
