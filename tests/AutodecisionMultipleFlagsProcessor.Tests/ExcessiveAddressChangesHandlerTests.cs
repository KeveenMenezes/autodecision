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
    public class ExcessiveAddressChangesHandlerTests
    {
        private readonly Mock<ILogger<ExcessiveAddressChangesHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public ExcessiveAddressChangesHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ExcessiveAddressChangesHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeIsRefiWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var excessiveAddressChangesHandler = new ExcessiveAddressChangesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = "2",
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = excessiveAddressChangesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFactorTrustAddressChangesLastTwoYearsIsNullWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var excessiveAddressChangesHandler = new ExcessiveAddressChangesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = "1",
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    AddressChangesLastTwoYears = null
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = excessiveAddressChangesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFactorTrustAddressChangesLastTwoYearsIsGreaterThanOneWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var excessiveAddressChangesHandler = new ExcessiveAddressChangesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = "1"
                },
                FactorTrust = new FactorTrust()
                {
                    AddressChangesLastTwoYears = 4
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = excessiveAddressChangesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFactorTrustAddressChangesLastTwoYearsIsGreaterThanOneAndWhiteListReasonIsEmptyOrNullWhenProcessingFlagThenFlagResultEqualsAutoDeny()
        {
            // Arrange
            var excessiveAddressChangesHandler = new ExcessiveAddressChangesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = "1",
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    AddressChangesLastTwoYears = 4
                },
                WhiteList = new WhiteList()
                {
                    Reason = ""
                }
            };
            var expected = FlagResultEnum.AutoDeny;

            // Act
            var actual = excessiveAddressChangesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFactorTrustAddressChangesLastTwoYearsIsOneAndApplicatonTypeIsNewLoanWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var excessiveAddressChangesHandler = new ExcessiveAddressChangesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = "1",
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    AddressChangesLastTwoYears = 1
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = excessiveAddressChangesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}