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
    public class FoundOnBlockListHandlerTests
    {
        private readonly Mock<ILogger<FoundOnBlockListHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public FoundOnBlockListHandlerTests()
        {
            _mockLogger = new Mock<ILogger<FoundOnBlockListHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenFoundOnBlockListTrueWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var foundOnBlockListHandler = new FoundOnBlockListHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                BlockList = new BlockList()
                {
                    Reason = "test",
                    CreatedAt = DateTime.Now
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = foundOnBlockListHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenFoundOnBlockListFalseWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var foundOnBlockListHandler = new FoundOnBlockListHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                BlockList = new BlockList()
                {
                    
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = foundOnBlockListHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

    }
}