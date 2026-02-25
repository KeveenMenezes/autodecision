using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using AutodecisionMultipleFlagsProcessor.Utility;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class CustomerAuthenticationHandler : IConsumer<ProcessFlagRequestEvent>
    {

        private readonly ILogger<CustomerAuthenticationHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private readonly ICustomerInfo _customerInfo;

        public CustomerAuthenticationHandler(ILogger<CustomerAuthenticationHandler> logger,
            IFlagHelper flagHelper,
            ICustomerInfo customerInfo
            )
        {
            _logger = logger;
            _flagHelper = flagHelper;
            _customerInfo = customerInfo;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);
			if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.CustomerAuthentication, _logger)) return;
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.CustomerAuthentication, autodecisionCompositeData, FlagResultEnum.Processed);
			try
            {
                if (!DefaultValidationsSucceeded(autodecisionCompositeData, response)) 
                    return response;
                
                var messages = new List<string>();
                if (!string.IsNullOrEmpty(autodecisionCompositeData.Census.FirstName))
                {
                    var customerFirstName = Util.RemoveSpecialCharacters(autodecisionCompositeData.Customer.FirstName.ToLower().Replace(" ", ""));
                    var censusFirstName = Util.RemoveSpecialCharacters(autodecisionCompositeData.Census.FirstName.ToLower().Replace(" ", ""));
                    var firstNameSimilarity = LevenshteinDistance.CalculateSimilarity(customerFirstName, censusFirstName);
                    if (firstNameSimilarity < autodecisionCompositeData.Census.FlagCustomerSimilarityValue)
                        messages.Add($"First name changed from {autodecisionCompositeData.Census.FirstName} to {autodecisionCompositeData.Customer.FirstName}");
                }
                if (!string.IsNullOrEmpty(autodecisionCompositeData.Census.LastName))
                {
                    var customerLastName = Util.RemoveSpecialCharacters(autodecisionCompositeData.Customer.LastName.ToLower().Replace(" ", ""));
                    var censusLastName = Util.RemoveSpecialCharacters(autodecisionCompositeData.Census.LastName.ToLower().Replace(" ", ""));
                    var lastNameSimilarity = LevenshteinDistance.CalculateSimilarity(customerLastName, censusLastName);
                    if (lastNameSimilarity < autodecisionCompositeData.Census.FlagCustomerSimilarityValue)
                        messages.Add($"Last name changed from {autodecisionCompositeData.Census.LastName} to {autodecisionCompositeData.Customer.LastName}");
                }
                if (autodecisionCompositeData.Census.DateOfBirth.HasValue && autodecisionCompositeData.Census.DateOfBirth != DateTime.MinValue && autodecisionCompositeData.Census.DateOfBirth.Value.Date != DateTime.Parse("1900-01-01"))
                {
                    if (autodecisionCompositeData.Census.DateOfBirth.Value.Date != autodecisionCompositeData.Customer.DateOfBirth.Value.Date)
                        messages.Add($"Date of birth changed from {autodecisionCompositeData.Census.DateOfBirth.Value.ToString("MM/dd/yyyy")} to {autodecisionCompositeData.Customer.DateOfBirth.Value.ToString("MM/dd/yyyy")}");
                    if (Util.IsDateOfBirthInValid(autodecisionCompositeData.Census.DateOfBirth.Value))
                        messages.Add($"Date of Birth is invalid - {autodecisionCompositeData.Census.DateOfBirth.Value.ToString("MM/dd/yyyy")} ");
                }
                if (!string.IsNullOrEmpty(autodecisionCompositeData.Census.EmployeeRegistration))
                {
                    var censusIsInt = int.TryParse(autodecisionCompositeData.Census.EmployeeRegistration, out var censusEmployeeRegistrationInt);
                    var customerIsInt = int.TryParse(autodecisionCompositeData.Customer.EmployeeRegistration, out var customerEmployeeRegistrationInt);
                    if (censusIsInt && customerIsInt)
                    {
                        if (censusEmployeeRegistrationInt != customerEmployeeRegistrationInt)
                            messages.Add($"Employee registration changed from {autodecisionCompositeData.Census.EmployeeRegistration} to {autodecisionCompositeData.Customer.EmployeeRegistration}");
                    }
                    else if (autodecisionCompositeData.Census.EmployeeRegistration != autodecisionCompositeData.Customer.EmployeeRegistration)
                            messages.Add($"Employee registration changed from {autodecisionCompositeData.Census.EmployeeRegistration} to {autodecisionCompositeData.Customer.EmployeeRegistration}");
                }
                if (messages.Count > 0)
                {
                    var description = string.Join(" | ", messages);
					_flagHelper.RaiseFlag(response, description);

                    if ((messages.Any(i => i.Contains("Last name changed from")) || messages.Any(i => i.Contains("First name changed from"))) && !messages.Any(i => i.Contains("Date of birth")) && !messages.Any(i => i.Contains("Employee registration changed from")) && autodecisionCompositeData.Application.Type == ApplicationType.Refi)
                    {
                        if (messages.Any(i => i.Contains("Last name changed from")) && (autodecisionCompositeData.Customer.LastName.Contains("-") || autodecisionCompositeData.Census.LastName.Contains("-")))
                        {
                            var customerLastName = (autodecisionCompositeData.Customer.LastName.ToLower().Replace(" ", ""));
                            var censusLastName = (autodecisionCompositeData.Census.LastName.ToLower().Replace(" ", ""));
                            if (customerLastName.Contains("-"))
                                customerLastName = customerLastName.Substring(0, customerLastName.IndexOf("-"));
                            if (censusLastName.Contains("-"))
                                censusLastName = censusLastName.Substring(0, censusLastName.IndexOf("-"));
                            var lastNameSimilarity = LevenshteinDistance.CalculateSimilarity(customerLastName, censusLastName);
                            if (lastNameSimilarity >= autodecisionCompositeData.Census.FlagCustomerSimilarityValue)
                            {
                                response.FlagResult = FlagResultEnum.Approved;
                                response.ApprovalNote = $"Name is hyphenated, same customer {customerLastName} & {censusLastName}";
                            }    

                        }
                        if (messages.Any(i => i.Contains("First name changed from")) && (autodecisionCompositeData.Customer.FirstName.Contains("-") || autodecisionCompositeData.Census.FirstName.Contains("-")))
                        {
                            var customerFirstName = (autodecisionCompositeData.Customer.FirstName.ToLower().Replace(" ", ""));
                            var censusFirstName = (autodecisionCompositeData.Census.FirstName.ToLower().Replace(" ", ""));
                            if (customerFirstName.Contains("-"))
                                customerFirstName = customerFirstName.Substring(0, customerFirstName.IndexOf("-"));
                            if (censusFirstName.Contains("-"))
                                censusFirstName = censusFirstName.Substring(0, censusFirstName.IndexOf("-"));
                            var lastNameSimilarity = LevenshteinDistance.CalculateSimilarity(customerFirstName, censusFirstName);
                            if (lastNameSimilarity >= autodecisionCompositeData.Census.FlagCustomerSimilarityValue)
                            {
                                response.FlagResult = FlagResultEnum.Approved;
                                response.ApprovalNote = $"Name is hyphenated, same customer {customerFirstName} & {censusFirstName}";
                            }
                        }
                    }
                }
                return response;
            }
            catch (Exception e)
            {
				_logger.LogError(e, $"FlagCode: {FlagCode.CustomerAuthentication} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

				response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private bool DefaultValidationsSucceeded(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response)
        {
            if (autodecisionCompositeData.Census?.FlagCustomerSimilarityValue == null)
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }
            if (autodecisionCompositeData.Census.CustomerId == 0)
            {
                _flagHelper.RaiseFlag(response, "Employee not found on census database.");
                return false;
            }
            if (autodecisionCompositeData.Census.FlagCustomerSimilarityValue == 0)
            {
                _flagHelper.RaiseFlag(response, "Customer Similarity range value not found on census database.");
                return false;
            }
            return true;
        }
    }
}