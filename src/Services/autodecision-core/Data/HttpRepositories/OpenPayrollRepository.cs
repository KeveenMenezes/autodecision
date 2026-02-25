using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Data.HttpRepositories.DTOs.Ocrolus;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using BMGMoney.SDK.V2.Http;

namespace AutodecisionCore.Data.HttpRepositories
{
    public class OpenPayrollRepository : IOpenPayrollRepository
    {
        private readonly IHttpService _httpService;
        private readonly ILogger<OpenPayrollRepository> _logger;

        public OpenPayrollRepository(
            IHttpService httpService,
            ILogger<OpenPayrollRepository> logger)
        {
            _httpService = httpService;
            _logger = logger;
        }

        public async Task<OpenPayroll> GetOpenPayrollConnections(int customerId, int applicationId, DateTime submittedAt)
        {
            try
            {
                var openPayrollConnections = new List<OpenPayrollConnection>();

                var openPayrollApiUrl = Environment.GetEnvironmentVariable("API_URL_OPEN_PAYROLL") + $"/api/v1/connection/application/{applicationId}";
                var headers = Token.GetHeaders();

                var resultOpenPayrolApi = await _httpService.GetAsync<NewOpenPayrollApiResultDTO>(openPayrollApiUrl, headers);

                if (resultOpenPayrolApi.Success && IsValidOpenPayrollApiConnection(resultOpenPayrolApi.Response))
                {
                    openPayrollConnections.Add(MapNewOpenPayrollConnection(resultOpenPayrolApi.Response.Data));
                }

                return new OpenPayroll { Connections = openPayrollConnections };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when trying to get Open Payroll information for CustomerId: {customerId} | Error: {ex.Message}");
                throw;
            }
        }

        private static decimal? GetPayAllocationValue(string allocationType, string allocationValue) =>
            PayAllocationType.HasNoAllocationValue(allocationType) ? null : (decimal.TryParse(allocationValue, out decimal value) ? value : null);


        #region New OpenPayrollApi
        private static OpenPayrollConnection MapNewOpenPayrollConnection(NewOpenPayrollDTO openPayroll)
        {
            var mappedConnection = new OpenPayrollConnection
            {
                ConnectedAt = DateTime.MinValue,
                IsActive = openPayroll.IsConnected,
                HireDate = openPayroll.HireDate,
                OldestPayDate = GetOldestPayDate(openPayroll.Payouts),
                HasMoreThanOneEmployer = PayoutHasMoreThanOneEmployer(openPayroll.Payouts),
                ProfileInformation = new ProfileInformation()
                {
                    Name = openPayroll.Name,
                    SSN = openPayroll.Ssn,
                    EmployerName = openPayroll.Employer,
                },
                PayAllocations = GetNewPayAllocationsMapped(openPayroll.PayAllocations),
                BmgAllotments = GetBmgAllotmentsMapped(openPayroll.BmgAllotments),
                VendorType = openPayroll.VendorType,
                OcrolusDocumentScore = openPayroll.OcrolusDocumentScore,
                OcrolusDocumentSignals = MapToOcrolusDocumentSignal(openPayroll.OcrolusDocumentSignals),
                OcrolusDocumentStatus = openPayroll.OcrolusDocumentStatusId,
                IsNewOpenPayroll = true
            };
            return mappedConnection;
        }

        private static bool PayoutHasMoreThanOneEmployer(List<PayoutsDTO> payouts)
        {
            var distinctEmployers = payouts
            .Select(p => p.Employer)
            .Distinct()
            .ToList();

            return distinctEmployers.Count > 1 && !ValidatePayoutEmployers(distinctEmployers);
        }

        private static DateTime GetOldestPayDate(List<PayoutsDTO> payouts)
        {
            var sortedListByDate = payouts.OrderBy(x => x.PayDate).ToList();
            return sortedListByDate.FirstOrDefault()?.PayDate ?? DateTime.MinValue;
        }

        private static bool ValidatePayoutEmployers(List<string> employers)
        {
            var firstEmployer = employers.FirstOrDefault();
            var lastEmployer = employers.LastOrDefault();

            if ((firstEmployer != "AMAZON.COM SERVICES LLC" && firstEmployer != "AMAZON.COM SERVICES, LLC") || lastEmployer != "amazon")
                return false;
            return true;
        }

        private static List<PayAllocations> GetNewPayAllocationsMapped(List<NewPayAllocation> newPayAllocations)
        {
            var payAllocations = new List<PayAllocations>();

            foreach (var payAllocation in newPayAllocations)
            {
                var payAllocationDto = new PayAllocations
                {
                    CreatedAt = payAllocation.CreatedAt.GetValueOrDefault(),
                    RoutingNumber = payAllocation.RoutingNumber,
                    AccountNumber = payAllocation.AccountNumber,
                    AccountType = payAllocation.AccountType,
                    IsRemainder = payAllocation.Remainder,
                    Value = GetPayAllocationValue(payAllocation.AllocationType, payAllocation.Amount)
                };
                payAllocations.Add(payAllocationDto);
            }
            return payAllocations;
        }

        private static List<BmgAllotments> GetBmgAllotmentsMapped(List<BmgAllotmentsDTO> bmgAllotmentsDTO)
        {
            var bmgAllotmentsList = new List<BmgAllotments>();

            foreach (var bmgAllotment in bmgAllotmentsDTO)
            {
                var bmgAllotments = new BmgAllotments
                {
                    Amount = bmgAllotment.Amount,
                    AccountNumber = bmgAllotment.AccountNumber,
                    RoutingNumber = bmgAllotment.RoutingNumber,
                    BankName = bmgAllotment.BankName,
                    AccountType = bmgAllotment.AccountType,
                    AccountName = bmgAllotment.AccountName,
                    CreatedAt = bmgAllotment.CreatedAt,
                    AllocationType = bmgAllotment.AllocationType,
                    Status = bmgAllotment.Status,
                    Value = GetPayAllocationValue(bmgAllotment.AllocationType, bmgAllotment.Amount)
                };
                bmgAllotmentsList.Add(bmgAllotments);
            }
            return bmgAllotmentsList;
        }

        private static bool IsValidOpenPayrollApiConnection(NewOpenPayrollApiResultDTO resultOpenPayrolApi)
        {
            return resultOpenPayrolApi != null
                && resultOpenPayrolApi.Success
                && resultOpenPayrolApi.Data.IsConnected;
        }

        private static List<OcrolusDocumentSignals> MapToOcrolusDocumentSignal(List<OcrolusDocumentSignalsDTO>? ocrolusDocSignalList)
        {
            var response = new List<OcrolusDocumentSignals>();
            if (ocrolusDocSignalList is null)
            {
                return response;
            }

            ocrolusDocSignalList.ForEach(ocrolusDocumentSignal =>
            {
                var responseObj = new OcrolusDocumentSignals()
                {
                    Reason = ocrolusDocumentSignal.Reason,
                    ConfidenceLevel = ocrolusDocumentSignal.ConfidenceLevel,
                };
                response.Add(responseObj);
            });

            return response;
        }

        #endregion
    }
}