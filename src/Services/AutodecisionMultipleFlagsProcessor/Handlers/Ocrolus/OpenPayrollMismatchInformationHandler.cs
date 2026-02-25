using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Helpers;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using System.Text.RegularExpressions;

namespace AutodecisionMultipleFlagsProcessor.Handlers.Ocrolus
{
    public class OpenPayrollMismatchInformationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<OpenPayrollMismatchInformationHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public OpenPayrollMismatchInformationHandler(
            ILogger<OpenPayrollMismatchInformationHandler> logger,
            IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.OpenPayrollMismatchInformation, _logger))
            {
                return;
            }

            var response = ProcessFlag(autodecisionComposite);
            response.Reason = context.Message.Reason;
            await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagCode.OpenPayrollMismatchInformation, autodecisionCompositeData, FlagResultEnum.Ignored);
            try
            {
                if (autodecisionCompositeData.OpenPayroll == null)
                {
                    return response;
                }

                var mostRecentlyActiveConnection = autodecisionCompositeData.OpenPayroll.Connections
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.ConnectedAt)
                    .FirstOrDefault();

                if (mostRecentlyActiveConnection == null)
                {
                    return response;
                }

                if (mostRecentlyActiveConnection.ProfileInformation is null)
                {
                    _flagHelper.RaiseFlag(response, "There is no profile information on open payroll connection to validate data!");
                    return response;
                }

                string? customerNameMessage = VerifyCustomerName(mostRecentlyActiveConnection.ProfileInformation.Name, autodecisionCompositeData.Customer.FirstName, autodecisionCompositeData.Customer.LastName);
                string? employerNameMessage = VerifyEmployerName(mostRecentlyActiveConnection.ProfileInformation.EmployerName, autodecisionCompositeData.Application.EmployerName);
                string? ssnMessage = VerifySSN(mostRecentlyActiveConnection.ProfileInformation.SSN, autodecisionCompositeData.Customer.Ssn);

                var flagMessage = FlagMessage(customerNameMessage, employerNameMessage, ssnMessage);
                if (string.IsNullOrEmpty(flagMessage))
                {
                    response.FlagResult = FlagResultEnum.Processed;
                    return response;
                }

                _flagHelper.RaiseFlag(response, flagMessage);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FlagCode: [{FlagCode}] was not successfully processed for LoanNumber: {LoanNumber} | Error: {ExMessage}", FlagCode.OpenPayrollMismatchInformation, autodecisionCompositeData.Application.LoanNumber, e.Message);

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;
                return response;
            }
        }

        private static string? VerifyCustomerName(string? openPayrollName, string? customerFirstName, string? customerLastName)
        {
            if (string.IsNullOrWhiteSpace(openPayrollName))
            {
                return "Customer name not found on open payroll data.";
            }

            var openPayrollRegex = Regex.Replace(openPayrollName, @"[^a-zA-Z\s]", "").ToUpper();
            var customerFullName = Regex.Replace(customerFirstName + " " + customerLastName, @"[^a-zA-Z\s]", "").ToUpper();

            if (openPayrollRegex != customerFullName)
            {
                return $"Customer names mismatch, open payroll data: [{openPayrollRegex}], customer information: [{customerFullName}]";
            }

            return null;
        }

        private static string? VerifyEmployerName(string? openPayrollEmployerName, string? employerName)
        {
            if (string.IsNullOrWhiteSpace(openPayrollEmployerName))
            {
                return "Employer name not found on open payroll data.";
            }

            if (string.IsNullOrWhiteSpace(employerName))
            {
                employerName = "";
            }

            var openPayrollRegex = Regex.Replace(openPayrollEmployerName, @"[^a-zA-Z\s]", "").ToUpper();
            var employerRegex = Regex.Replace(employerName, @"[^a-zA-Z\s]", "").ToUpper();

            if (openPayrollRegex != employerRegex)
            {
                return $"Employer names mismatch, open payroll data: [{openPayrollRegex}], customer information: [{employerRegex}]";
            }

            return null;
        }

        private static string? VerifySSN(string? openPayrollSSN, string? customerSSN)
        {
            if (string.IsNullOrWhiteSpace(openPayrollSSN))
            {
                return "SSN not found on open payroll data.";
            }

            if (string.IsNullOrWhiteSpace(customerSSN))
            {
                customerSSN = "";
            }

            string lastFourVendor = GetLastFour(openPayrollSSN);
            if (lastFourVendor.Contains("*"))
            {
                return null;
            }

            var lastFourCustomer = GetLastFour(customerSSN);

            if (lastFourVendor == lastFourCustomer)
            {
                return null;
            }

            return $"SSN last four mismatch, open payroll data: [{lastFourVendor}], customer information: [{lastFourCustomer}]";
        }

        private static string GetLastFour(string source)
        {
            if (source.Length <= 4)
            {
                return source;
            }

            return source.Substring(source.Length - 4);
        }

        private static string? FlagMessage(string? message1, string? message2, string? message3)
        {
            var messageList = new List<string>();
            if (!string.IsNullOrWhiteSpace(message1))
            {
                messageList.Add(message1);
            }

            if (!string.IsNullOrWhiteSpace(message2))
            {
                messageList.Add(message2);
            }

            if (!string.IsNullOrWhiteSpace(message3))
            {
                messageList.Add(message3);
            }

            if (messageList.Count == 0)
                return null;

            return string.Join(", ", messageList);
        }
    }
}