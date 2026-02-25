using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class HighestCreditLimitHandlerTests
    {
        private readonly Mock<ILogger<HighestCreditLimitHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public HighestCreditLimitHandlerTests()
        {
            _mockLogger = new Mock<ILogger<HighestCreditLimitHandler>>();
            _mockFlagHelper =  new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeIsRefiWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var highestCreditLimitHandler = new HighestCreditLimitHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.Refi,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = highestCreditLimitHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFactorTrustBalanceToIncomeIsNullWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var highestCreditLimitHandler = new HighestCreditLimitHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    BalanceToIncome = null
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = highestCreditLimitHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBalanceToIncomeGreaterThanMaxValueAndWhiteListReasonNullOrEmptyWhenProcessingFlagThenFlagResultEqualsAutoDeny()
        {
            // Arrange
            var highestCreditLimitHandler = new HighestCreditLimitHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    BalanceToIncome = 3
                },
                WhiteList = new WhiteList()
                {
                    Reason = ""
                }
            };
            var expected = FlagResultEnum.AutoDeny;

            // Act
            var actual = highestCreditLimitHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBalanceToIncomeGreaterThanMaxValueAndWhiteListReasonNotNullOrEmptyWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var highestCreditLimitHandler = new HighestCreditLimitHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    BalanceToIncome = 3
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = highestCreditLimitHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBalanceToIncomeIsLowerThenMaxValueWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var highestCreditLimitHandler = new HighestCreditLimitHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    BalanceToIncome = 1
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = highestCreditLimitHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}