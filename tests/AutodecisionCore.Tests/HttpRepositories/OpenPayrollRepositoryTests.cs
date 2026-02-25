using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Data.HttpRepositories;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using BMGMoney.SDK.V2.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace AutodecisionCore.Tests.HttpRepositories
{
    public class OpenPayrollRepositoryTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpService> _mockHttpService;
        private readonly Mock<ILogger<OpenPayrollRepository>> _mockLogger;

        public OpenPayrollRepositoryTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpService = new Mock<IHttpService>();
            _mockLogger = new Mock<ILogger<OpenPayrollRepository>>();
        }

        private List<OpenPayrollDTO> ArrangeOpenPayrollData()
        {
            return new List<OpenPayrollDTO>
            {
                new OpenPayrollDTO
                {
                    Name = "Mr Tested",
                    Ssn = "1234-5678",
                    IsActive = true,
                    ConnectionUpdatedAt = DateTime.Now,
                    PayAllocation = new PayAllocationsDTO()
                    {
                        EmployerId = 123,
                        PayAllocations = new List<PayAllocation>()
                        {
                            new PayAllocation(
                                createdAt: DateTime.Now,
                                bankRoutingNumber: "*****2222",
                                bankAccountNumber: "*****1111",
                                bankAccountType: "savings",
                                allocationType: "amount",
                                allocationValue: "121.95"),
                            new PayAllocation(
                                createdAt: DateTime.Now,
                                bankRoutingNumber: "*****1111",
                                bankAccountNumber: "*****2222",
                                bankAccountType: "checking",
                                allocationType: "remainder",
                                allocationValue: null),
                        }
                    }
                }
            };
        }

        private static List<PayoutsDTO> ArrangeLastThreePayoutsDto(List<PayoutsDTO> payouts)
        {
            var lastPayouts = new List<PayoutsDTO>();

            var sortedListByDate = payouts.OrderByDescending(x => x.PayDate).ToList();
            var lastThreePayouts = sortedListByDate.Count >= 3 ? sortedListByDate.Take(3) : sortedListByDate;

            foreach (var item in lastThreePayouts)
            {
                var payoutDto = new PayoutsDTO
                {
                    NetPay = item.NetPay,
                    GrossPay = item.GrossPay,
                    PayDate = item.PayDate
                };

                lastPayouts.Add(payoutDto);
            }

            return lastPayouts;
        }

        private void AssertOpenPayrollConnection(OpenPayrollDTO expected, OpenPayrollConnection actual)
        {
            Assert.Equal(expected.Name, actual.ProfileInformation.Name);
            Assert.Equal(expected.Ssn, actual.ProfileInformation.SSN);
            Assert.Equal(expected.IsActive, actual.IsActive);


            for (int i = 0; i < expected.PayAllocation.PayAllocations.Count; i++)
            {
                Assert.Equal(expected.PayAllocation.PayAllocations[i].BankAccountType, actual.PayAllocations[i].AccountType);
                Assert.Equal(expected.PayAllocation.PayAllocations[i].BankAccountNumber, actual.PayAllocations[i].AccountNumber);
                Assert.Equal(expected.PayAllocation.PayAllocations[i].BankRoutingNumber, actual.PayAllocations[i].RoutingNumber);
                Assert.Equal(expected.PayAllocation.PayAllocations[i].AllocationType == "remainder", actual.PayAllocations[i].IsRemainder);
                Assert.Equal(expected.PayAllocation.PayAllocations[i].CreatedAt, actual.PayAllocations[i].CreatedAt);
            }
        }
    }
}
