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
    public class TUCriticalAndInternetBankHandlerTests
    {
        private readonly Mock<ILogger<TUCriticalAndInternetBankHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public TUCriticalAndInternetBankHandlerTests()
        {
            _mockLogger = new Mock<ILogger<TUCriticalAndInternetBankHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeIsRefiWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.Refi
                }
            };
            var expected = FlagResultEnum.Ignored;
            
            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFundingMethodIsCheckWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test",
                    FundingMethod = ApplicationFundingMethod.Check
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenTransunionResultScoreIsEqualOrGreaterThan30WhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    FundingMethod = ApplicationFundingMethod.DebitCard,
                    LoanNumber = "test"
                },
                TransunionResult = new TransunionResult()
                {
                    Score = 30
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenApplicationProductIdEqualsProductAtWorkWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    FundingMethod = ApplicationFundingMethod.DebitCard,
                    ProductId = ApplicationProductId.ProductAtWork,
                    LoanNumber = "test"
                },
                TransunionResult = new TransunionResult()
                {
                    Score = 29
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBankIsNotHighRiskWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    ProductId = ApplicationProductId.Standard,
                    Type = ApplicationType.NewLoan,
                    FundingMethod = ApplicationFundingMethod.DebitCard,
                    LoanNumber = "test",
                    HighRisk = false
                },
                TransunionResult = new TransunionResult()
                {
                    Score = 29
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBankIsHighRiskAndWhiteListReasonIsNullOrEmptyWhenProcessingFlagThenFlagResultEqualsAutoDeny()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    ProductId = ApplicationProductId.Standard,
                    Type = ApplicationType.NewLoan,
                    FundingMethod = ApplicationFundingMethod.DebitCard,
                    LoanNumber = "test",
                    HighRisk = true,
                    TurndownActive = true
                },
                TransunionResult = new TransunionResult()
                {
                    Score = 29
                },
                WhiteList = new WhiteList()
                {
                    Reason = ""
                }
            };
            var expected = FlagResultEnum.AutoDeny;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenBankIsHighRiskAndWhiteListReasonIsNotNullOrEmptyWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var tuCriticalAndInternetBankHandler = new TUCriticalAndInternetBankHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    ProductId = ApplicationProductId.Standard,
                    Type = ApplicationType.NewLoan,
                    FundingMethod = ApplicationFundingMethod.DebitCard,
                    LoanNumber = "test",
                    HighRisk = true
                },
                TransunionResult = new TransunionResult()
                {
                    Score = 29
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = tuCriticalAndInternetBankHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

    }
}