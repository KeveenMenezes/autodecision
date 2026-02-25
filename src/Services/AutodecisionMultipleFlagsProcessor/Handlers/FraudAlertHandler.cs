using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;
using System.Text.RegularExpressions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class FraudAlertHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<FraudAlertHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly IFeatureToggleClient _featureToggleClient;

        public FraudAlertHandler(ILogger<FraudAlertHandler> logger, IFlagHelper flagHelper, IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _featureToggleClient = featureToggleClient;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.FraudAlert, _logger)) return;
            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData data)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.FraudAlert, data, FlagResultEnum.Processed);

            if (_featureToggleClient.IsDisabled("fraud_flag_autodecision"))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return response;
            }

            if (data.Application.Type == ApplicationType.Refi)
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return response;
            }

            try
            {
                //No Factor Trust Data
                if (data.FactorTrust.RiskScore == -1 && data.FactorTrust.AddressChangesLastTwoYears == -1)
                {

                    _flagHelper.RaiseFlag(response, "No Factor Trust Data Return");
                    return response;
                }
                var listFraudAlert = GetMatchingVariables(data.FactorTrust);
                if (listFraudAlert.Count == 0) return response;

                string resultString = "";
                foreach (var variable in listFraudAlert)
                    resultString = resultString + $"{string.Join(" ", Regex.Split(variable, @"(?<!^)(?=[A-Z])"))}: {data.FactorTrust.GetType().GetProperty(variable).GetValue(data.FactorTrust)}; ";


                _flagHelper.RaiseFlag(response, resultString);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlagCode: {FlagCode.FraudAlert} was not successfully processed for LoanNumber: {data.Application.LoanNumber} | Error: {ex.Message}");
                response.Message = ex.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }


        public static List<string> GetMatchingVariables(FactorTrust factorTrust)
        {
            var matchingVariables = new List<string>();

            if (factorTrust.TcaAlert == "1") matchingVariables.Add("TcaAlert");
            if (factorTrust.TcaDeceased == "1") matchingVariables.Add("TcaDeceased");
            if (factorTrust.TcaActiveMilitaryDutyAlert == "1") matchingVariables.Add("TcaActiveMilitaryDutyAlert");
            if (!string.IsNullOrEmpty(factorTrust.TcaTrueNameFraudText) && factorTrust.TcaTrueNameFraudText != "-1") matchingVariables.Add("TcaTrueNameFraudText");
            if (!string.IsNullOrEmpty(factorTrust.TcaSecurityAlertText) && factorTrust.TcaSecurityAlertText != "-1") matchingVariables.Add("TcaSecurityAlertText");
            if (!string.IsNullOrEmpty(factorTrust.TcaInitialFraudAlertText) && factorTrust.TcaInitialFraudAlertText != "-1") matchingVariables.Add("TcaInitialFraudAlertText");
            if (!string.IsNullOrEmpty(factorTrust.TcaExtendedFraudAlertText) && factorTrust.TcaExtendedFraudAlertText != "-1") matchingVariables.Add("TcaExtendedFraudAlertText");
            if (!string.IsNullOrEmpty(factorTrust.TcaActiveMilitaryDutyAlertText) && factorTrust.TcaActiveMilitaryDutyAlertText != "-1") matchingVariables.Add("TcaActiveMilitaryDutyAlertText");

            return matchingVariables;
        }

    }
}
