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
    public class NoTUResponseHandlerTests
    {
        private readonly Mock<ILogger<NoTUResponseHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public NoTUResponseHandlerTests()
        {
            _mockLogger = new Mock<ILogger<NoTUResponseHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeIsRefiWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var noTUResponseHandler = new NoTUResponseHandler(_mockLogger.Object, _mockFlagHelper.Object);
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
            var actual = noTUResponseHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenTransunionResultIsNullWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var noTUResponseHandler = new NoTUResponseHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = null
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = noTUResponseHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenTransunionResultSessionIdIsNegativeOneWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var noTUResponseHandler = new NoTUResponseHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    SessionId = "-1"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = noTUResponseHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenNotValidatedClarityWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var noTUResponseHandler = new NoTUResponseHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult() { },
                Clarity = new Clarity() 
                {
                    DobMatchResult = "test",
                    DobMatchDescription = "test",
                    NameAddressMatchConfidence = 0,
                    NameAddressMatchDescription = "test",
                    SSNNameAddressMatchConfidence = 0,
                    SSNNameAddressMatchDescription = "test"
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = noTUResponseHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenValidatedClarityWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var noTUResponseHandler = new NoTUResponseHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    SessionId = "-1"
                },
                Clarity = new Clarity() 
                {
                    DobMatchResult = "Match",
                    DobMatchDescription = "Full DOB available and matched input (within +/- 1 year if month.day exact)",
                    NameAddressMatchConfidence = 5,
                    NameAddressMatchDescription = "Exact match on first and last name; Exact match on address",
                    SSNNameAddressMatchConfidence = 5,
                    SSNNameAddressMatchDescription = "Exact SSN match, Exact Name match, Exact Address match"
                }
            };
            var expected = FlagResultEnum.Approved;

            // Act
            var actual = noTUResponseHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

    }
}