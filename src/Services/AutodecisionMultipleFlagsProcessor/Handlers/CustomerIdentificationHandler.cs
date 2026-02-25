using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Utility;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class CustomerIdentificationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly ILogger<CustomerIdentificationHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public CustomerIdentificationHandler(ILogger<CustomerIdentificationHandler> logger,
            IFlagHelper flagHelper,
              IFeatureToggleClient featureToggleClient
            )
        {
            _logger = logger;
            _featureToggleClient = featureToggleClient;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.CustomerIdentityFlag, _logger))
                return;

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;

            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData compositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.CustomerIdentityFlag, compositeData, FlagResultEnum.Processed);

            try
            {
                if (_featureToggleClient.IsEnabled("new_autodecision_ignore_some_flags"))
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                bool isMandatory = compositeData.CreditPolicy.EmployerRules.EmployerRulesItems.Any(x => x.Key == NewCreditPolicyRule.FaceIdMandatory);
                if (!isMandatory)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    return response;
                }

                var activeStatusList = new[]
                {
                    ApplicationStatus.Booked,
                    ApplicationStatus.Liquidated
                };

                var lastApplication = compositeData.LastApplications.FirstOrDefault(a => activeStatusList.Contains(a.Status));

                if (lastApplication != null && lastApplication.HasCustomerIdentiyValidated)
                {
                    response.FlagResult = FlagResultEnum.Approved;
                    response.ApprovalNote = "Approved due to last application.";
                    return response;
                }

                if (!OpenConnectionsHelper.IsFaceRecognitionDone(compositeData.FaceRecognition))
                    _flagHelper.RaiseFlag(response, "Face Recognition pending.");

                if (!(compositeData.FaceRecognition?.Liveness).GetValueOrDefault(false) ||
                    !(compositeData.FaceRecognition?.DocumentScanSuccess).GetValueOrDefault(false))
                    _flagHelper.RaiseFlag(response, "Face Recognition is not completed.");

                if (compositeData.FaceRecognition?.FraudStatus?.ToUpper() != "DONE")
                    _flagHelper.RaiseFlag(response, "Face match not completed");

                if (compositeData.FaceRecognition?.FraudStatus?.ToUpper() == "DONE"
                    && compositeData.FaceRecognition.ClientIdsMatch is not null
                    && compositeData.FaceRecognition.ClientIdsMatch.Any(x => x > 0))
                    _flagHelper.RaiseFlag(response, "Face match found for another customer");

                return response;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private void ValidateCustomersNameMatchCustomerNameOnDocument(ProcessFlagResponseEvent response, AutodecisionCompositeData autodecisionCompositeData)
        {
            string customerFirstName = autodecisionCompositeData.Customer.FirstName;
            string customerLastName = autodecisionCompositeData.Customer.LastName;

            string? customerFirstNameOnDocument = autodecisionCompositeData.FaceRecognition?.DocumentData?.FirstName;
            string? customerLastNameOnDocument = autodecisionCompositeData.FaceRecognition?.DocumentData?.LastName;

            if (autodecisionCompositeData.FaceRecognition?.DocumentData is null
                || string.IsNullOrEmpty(customerFirstNameOnDocument) || string.IsNullOrEmpty(customerLastNameOnDocument))
            {
                _flagHelper.RaiseFlag(response, "Document data couldn't be verified automatically. Please check it manually.");
            }

            if (!Util.StandardizeString(customerFirstNameOnDocument).StartsWith(Util.StandardizeString(customerFirstName))
                || !Util.StandardizeString(customerLastNameOnDocument).StartsWith(Util.StandardizeString(customerLastName)))
            {
                _flagHelper.RaiseFlag(response, "Customer's name doesn't match the customer's name on their document.");
            }
        }

    }
}
