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
    public class FoundOnWhiteListHandlerTests
    {
        private readonly Mock<ILogger<FoundOnWhiteListHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public FoundOnWhiteListHandlerTests()
        {
            _mockLogger = new Mock<ILogger<FoundOnWhiteListHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenFoundOnWhiteListTrueWhenProcessingFlagThenResultEqualsPendingApproval()
        {
            // Arrange
            var foundOnWhiteListHandler = new FoundOnWhiteListHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test",
                    CreatedAt = DateTime.Now
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = foundOnWhiteListHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFoundOnWhiteListFalseWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var foundOnWhiteListHandler = new FoundOnWhiteListHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                WhiteList = new WhiteList()
                {
                    
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = foundOnWhiteListHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}