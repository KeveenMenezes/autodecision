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

    public class DifferentStatesGeolocationHandlerTests 
    {

        private readonly Mock<ILogger<DifferentStatesGeolocationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public DifferentStatesGeolocationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<DifferentStatesGeolocationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenANullStateAbbreviationWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var DifferentStatesGeolocationHandler = new DifferentStatesGeolocationHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    StateAbbreviation = null,
                    LoanNumber = "test"
                }
            };

            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = DifferentStatesGeolocationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenANullStateIpUserRequestWhenProcessingFlagThenFlagResultEqualsIgnored()
        {

            // Arrange
            var DifferentStatesGeolocationHandler = new DifferentStatesGeolocationHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    StateIpUserRequest = null,
                    LoanNumber = "test"
                }
            };

            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = DifferentStatesGeolocationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenAnApplicationThatIsNotANewLoanWhenProcessingFlagThenFlagResultEqualsIgnored()
        {

            // Arrange
            var DifferentStatesGeolocationHandler = new DifferentStatesGeolocationHandler(_mockLogger.Object, _mockFlagHelper.Object);

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
            var actual = DifferentStatesGeolocationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenStateAbbreviationDifferentFromStateIpUserRequestWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {

            // Arrange
            var DifferentStatesGeolocationHandler = new DifferentStatesGeolocationHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    StateAbbreviation = "FL",
                    StateIpUserRequest = "CA",
                    LoanNumber = "test"
                }
            };

            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = DifferentStatesGeolocationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenAllCorrectDataWhenProcessingFlagThenFlagResultEqualsProcessed()
        {

            // Arrange 
            var DifferentStatesGeolocationHandler = new DifferentStatesGeolocationHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    StateAbbreviation = "FL",
                    StateIpUserRequest = "FL",
                    LoanNumber = "test"
                }
            };
        
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = DifferentStatesGeolocationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);

        }

    }

}
