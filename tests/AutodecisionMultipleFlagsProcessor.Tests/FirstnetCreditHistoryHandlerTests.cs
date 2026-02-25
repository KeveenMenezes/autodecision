using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class FirstnetCreditHistoryHandlerTests
    {
        private readonly Mock<ILogger<FirstnetCreditHistoryHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public FirstnetCreditHistoryHandlerTests()
        {
            _mockLogger = new Mock<ILogger<FirstnetCreditHistoryHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeIsNotRefiWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var firstnetCreditHistoryHandler = new FirstnetCreditHistoryHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = firstnetCreditHistoryHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenLastApplicationIsFirstnetPassThruWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var firstnetCreditHistoryHandler = new FirstnetCreditHistoryHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.Refi,
                    LoanNumber = "test"
                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication() { ReconciliationSystem = "firstnetpassthru", CreatedAt = DateTime.Now }
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = firstnetCreditHistoryHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenCheckFirstnetCreditReturnsTrueWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var firstnetCreditHistoryHandler = new FirstnetCreditHistoryHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.Refi,
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1
                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication() { ReconciliationSystem = "test", CreatedAt = DateTime.Now }
                }
            };
            _mockCustomerInfo.Setup(mock => mock.CheckFirstnetCredit(1, 20)).ReturnsAsync(true);
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = firstnetCreditHistoryHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenCheckFirstnetCreditReturnsFalseApplicationTypeEqualsRefiWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var firstnetCreditHistoryHandler = new FirstnetCreditHistoryHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.Refi,
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1
                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication() { ReconciliationSystem = "test", CreatedAt = DateTime.Now }
                }
            };
            _mockCustomerInfo.Setup(mock => mock.CheckFirstnetCredit(1, 20)).ReturnsAsync(false);
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = firstnetCreditHistoryHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }

}