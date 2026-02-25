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
    public class DifferentStatesHandlerTests 
    {
        private readonly Mock<ILogger<DifferentStatesHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public DifferentStatesHandlerTests()
        {
            _mockLogger = new Mock<ILogger<DifferentStatesHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenANullStateAbbreviationWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var differentStatesHandler = new DifferentStatesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    StateAbbreviation = null,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = differentStatesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenDifferentStateAbbreviationAndLoanTermsStateAbbreviationWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var differentStatesHandler = new DifferentStatesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    StateAbbreviation = "FL",
                    LoanTermsStateAbbreviation = "CA",
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = differentStatesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenWarningsFoundWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var differentStatesHandler = new DifferentStatesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    StateAbbreviation = "FL",
                    LoanTermsStateAbbreviation = "FL",
                    LoanNumber = "test"
                },
                ApplicationWarnings = new List<ApplicationWarning>()
                {
                    new ApplicationWarning() { Description = "test", Type = "11" }
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = differentStatesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenAllValidDataWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var differentStatesHandler = new DifferentStatesHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    StateAbbreviation = "FL",
                    LoanTermsStateAbbreviation = "FL",
                    LoanNumber = "test"
                },
                ApplicationWarnings = new List<ApplicationWarning>() { }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = differentStatesHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

    }
}