using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Client;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace AutodecisionMultipleFlagsProcessor.Tests 
{
    public class InternetBankHandlerTests 
    {
        private readonly Mock<ILogger<InternetBankHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<IFeatureToggleClient> _featureToggleClient;

        public InternetBankHandlerTests() 
        {
            _mockLogger = new Mock<ILogger<InternetBankHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _featureToggleClient = new Mock<IFeatureToggleClient>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenFundingMethodEqualsCheckWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var InternetBankHandler = new InternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object, _featureToggleClient.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = ApplicationFundingMethod.Check,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;
            
            // Act
            var actual = InternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenProductIdEqualsProductAtWorkWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var InternetBankHandler = new InternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object, _featureToggleClient.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    ProductId = ApplicationProductId.ProductAtWork,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = InternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBankIsHighRiskWhenProcessingFlagThenResultEqualsPendingApproval()
        {
            // Arrange
            var internetBankHandler = new InternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object, _featureToggleClient.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = ApplicationFundingMethod.ACH,
                    ProductId = ApplicationProductId.Standard,
                    LoanNumber = "test",
                    HighRisk = true
                },
                Customer = new Customer()
                {
                    BankName = "test"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = internetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenSameBankInfoAsPreviousLoanWhenProcessingFlagThenFlagResultEqualsApproved()
        {
            // Arrange
            var internetBankHandler = new InternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object, _featureToggleClient.Object);
            var mockLastApplicationFoo = new LastApplication()
            {
                LoanNumber = "123",
                BankAccountNumber = "123",
                BankRoutingNumber = "123",
                CreatedAt = DateTime.Now,
                Id = 123
            };
            List<LastApplication> lastApplicationsList = new List<LastApplication>();
            lastApplicationsList.Add(mockLastApplicationFoo);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = ApplicationFundingMethod.ACH,
                    ProductId = ApplicationProductId.Standard,
                    LoanNumber = "test",
                    BankAccountNumber = "123",
                    BankRoutingNumber = "123",
                    HighRisk = true,
                    PreviousApplicationId = 123
                },
                LastApplications = lastApplicationsList,
                Customer = new Customer()
                {
                    BankName = "test"
                }
            };
            var expected = FlagResultEnum.Approved;

            // Act
            var actual = internetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);

        }

    }
}