using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class OpenBankingOpenPayrollBankAccountRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;

        public OpenBankingOpenPayrollBankAccountRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();


        }

        //[Fact]
        //public void GivenAOpenPayrollBankAccountThatMatchesThenResultEqualsApproved()
        //{

        //    // Arrange
        //    var openBankingOpenPayrollBankAccountRule = new OpenBankingOpenPayrollBankAccountRule(_mockFeatureToggle.Object, _mockLoger.Object);


        //    var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();
        //    mockAutoApprovalRequest.LoanNumber = "123456";
        //    mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
        //    {
        //        Type = ApplicationType.Refi,
        //        Program = ApplicationProgram.LoansForAll,
        //        FundingMethod = "debit_card",
        //        PaymentType = "ach",
        //        UwCluster = "A",
        //        AmountOfPayment = 115m
        //    };

        //    var payAllocations1 = new PayAllocations()
        //    {
        //        IsRemainder = true,
        //        RoutingNumber = "*****2345",
        //        AccountNumber = "*****2346"

        //    };
        //    var payAllocations2 = new PayAllocations()
        //    {
        //        IsRemainder = false,
        //        RoutingNumber = "*****2347",
        //        AccountNumber = "*****2348"
        //    };

        //    var openPayrollConnection1 = new OpenPayrollConnection()
        //    {
        //        IsActive = true,
        //        PayAllocations = new List<PayAllocations>
        //        {
        //            payAllocations1,
        //            payAllocations2
        //        }
        //    };
            
        //    var openPayrollConnection2 = new OpenPayrollConnection()
        //    {
        //        IsActive = false,
        //        PayAllocations = new List<PayAllocations>
        //        {
        //            payAllocations1,
        //            payAllocations2
        //        }
        //    };

        //    var openBankingConnection = new OpenBankingConnections()
        //    {
        //        RoutingNumber = "000012345",
        //        AccountNumber = "2346",
        //        IsDefault = true
        //    };

        //    var openPayroll = new OpenPayroll()
        //    {
        //        Connections = new List<OpenPayrollConnection>()
        //        {
        //            openPayrollConnection1,
        //            openPayrollConnection2
                    
        //        }
        //    };
        //    var openBanking = new OpenBanking()
        //    {
        //        Connections = new List<OpenBankingConnections>()
        //        {
        //            openBankingConnection
        //        }
        //    };

        //    mockAutoApprovalRequest.OpenBanking = openBanking;
        //    mockAutoApprovalRequest.OpenPayroll = openPayroll;


        //    var response = new AutoApprovalResponse();

        //    var expected = AutoApprovalResultEnum.Approved;

        //    // Act
        //    openBankingOpenPayrollBankAccountRule.RunRule(mockAutoApprovalRequest, response, null);

        //    // Assert
        //    Assert.Equal(response.AutoApprovalRules.Last().Status, expected);
        //}

        [Fact]
        public void GivenAOpenPayrollBankAccountThatDoesNotMatchThenResultEqualsPending()
        {
            // Arrange
            var openBankingOpenPayrollBankAccountRule = new OpenBankingOpenPayrollBankAccountRule(_mockFeatureToggle.Object, _mockLoger.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();
            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 115m
            };

            var payAllocations1 = new PayAllocations()
            {
                IsRemainder = true,
                RoutingNumber = "*****2345",
                AccountNumber = "*****2346"

            };
            var payAllocations2 = new PayAllocations()
            {
                IsRemainder = false,
                RoutingNumber = "*****2347",
                AccountNumber = "*****2348"
            };

            var openPayrollConnection1 = new OpenPayrollConnection()
            {
                IsActive = true,
                PayAllocations = new List<PayAllocations>
                {
                    payAllocations1,
                    payAllocations2
                }
            };

            var openPayrollConnection2 = new OpenPayrollConnection()
            {
                IsActive = false,
                PayAllocations = new List<PayAllocations>
                {
                    payAllocations1,
                    payAllocations2
                }
            };

            var openBankingConnection = new OpenBankingConnections()
            {
                RoutingNumber = "000012344",
                AccountNumber = "2344",
                IsDefault = true
            };

            var openPayroll = new OpenPayroll()
            {
                Connections = new List<OpenPayrollConnection>()
                {
                    openPayrollConnection1,
                    openPayrollConnection2

                }
            };
            var openBanking = new OpenBanking()
            {
                Connections = new List<OpenBankingConnections>()
                {
                    openBankingConnection
                }
            };

            mockAutoApprovalRequest.OpenBanking = openBanking;
            mockAutoApprovalRequest.OpenPayroll = openPayroll;

            var mockApplicationCore = new ApplicationCore("111111");

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            openBankingOpenPayrollBankAccountRule.RunRule(mockAutoApprovalRequest, response, mockApplicationCore);

            // Assert
            Assert.Equal(response.AutoApprovalRules.Last().Status, expected);
        }

        //[Fact]
        //public void GivenEmptyPayAllocationsThenResultEqualsDenied()
        //{
        //    // Arrange
        //    var openBankingOpenPayrollBankAccountRule = new OpenBankingOpenPayrollBankAccountRule(_mockFeatureToggle.Object, _mockLoger.Object);

        //    var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();
        //    mockAutoApprovalRequest.LoanNumber = "123456";
        //    mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
        //    {
        //        Type = ApplicationType.Refi,
        //        Program = ApplicationProgram.LoansForAll,
        //        FundingMethod = "debit_card",
        //        PaymentType = "ach",
        //        UwCluster = "A",
        //        AmountOfPayment = 115m
        //    };

        //    var payAllocations1 = new PayAllocations()
        //    {
        //        IsRemainder = true,
        //        RoutingNumber = "*****2345",
        //        AccountNumber = "*****2346"

        //    };
        //    var payAllocations2 = new PayAllocations()
        //    {
        //        IsRemainder = false,
        //        RoutingNumber = "*****2347",
        //        AccountNumber = "*****2348"
        //    };

        //    var openPayrollConnection1 = new OpenPayrollConnection()
        //    {
        //        IsActive = true,
        //        PayAllocations = new List<PayAllocations>
        //        {
                    
        //        }
        //    };

        //    var openPayrollConnection2 = new OpenPayrollConnection()
        //    {
        //        IsActive = false,
        //        PayAllocations = new List<PayAllocations>
        //        {
                    
        //        }
        //    };

        //    var openBankingConnection = new OpenBankingConnections()
        //    {
        //        RoutingNumber = "000012345",
        //        AccountNumber = "2346",
        //        IsDefault = true
        //    };

        //    var openPayroll = new OpenPayroll()
        //    {
        //        Connections = new List<OpenPayrollConnection>()
        //        {
        //            openPayrollConnection1,
        //            openPayrollConnection2

        //        }
        //    };
        //    var openBanking = new OpenBanking()
        //    {
        //        Connections = new List<OpenBankingConnections>()
        //        {
        //            openBankingConnection
        //        }
        //    };

        //    mockAutoApprovalRequest.OpenBanking = openBanking;
        //    mockAutoApprovalRequest.OpenPayroll = openPayroll;

        //    var response = new AutoApprovalResponse();

        //    var expected = AutoApprovalResultEnum.Pending;

        //    // Act
        //    openBankingOpenPayrollBankAccountRule.RunRule(mockAutoApprovalRequest, response, null);

        //    // Assert
        //    Assert.Equal(response.AutoApprovalRules.Last().Status, expected);
        //}

        [Theory]
        [InlineData("12345678", "12345678", "87654321", "87654321", true)]
        [InlineData("12345678", "12345679", "87654321", "87654321", false)]
        [InlineData("12345678", "12345678", "87654321", "87654322", false)]
        [InlineData("***5678", "12345678", "87654321", "87654321", true)]
        [InlineData("12345678", "****5678", "87654321", "87654321", false)]
        [InlineData("12345678", "12345678", "****4321", "87654321", true)]
        [InlineData("12345678", "12345678", "87654321", "****4321", false)]
        [InlineData("***5679", "12345678", "87654321", "87654321", false)]
        [InlineData("12345678", "****5679", "87654321", "87654321", false)]
        [InlineData("12345678", "12345678", "****4322", "87654321", false)]
        [InlineData("12345678", "12345678", "87654321", "****4322", false)]
        [InlineData("012345678", "12345678", "87654321", "87654321", false)]
        [InlineData("12345678", "012345678", "87654321", "87654321", true)]
        [InlineData("12345678", "12345678", "087654321", "87654321", false)]
        [InlineData("12345678", "12345678", "87654321", "087654321", true)]
        public void CompareTwoBankRoutingAndAccount(string routingNumber1, string routingNumber2, string accountNumber1, string accountNumber2, bool expectedResult)
        {
            var openBankingOpenPayrollBankAccountRule = new OpenBankingOpenPayrollBankAccountRule(_mockFeatureToggle.Object, _mockLoger.Object);

			var accountNumberFormatted = Regex.Match(accountNumber1, @"\d+").Value;
			var routingNumberFormatted = Regex.Match(routingNumber1, @"\d+").Value;

			var result1 = routingNumber2.EndsWith(routingNumberFormatted);
            var result2 = accountNumber2.EndsWith(accountNumberFormatted);

            Assert.Equal(expectedResult, result1 && result2);
        }
    }
}
