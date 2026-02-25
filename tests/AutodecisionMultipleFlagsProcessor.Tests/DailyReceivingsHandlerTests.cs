using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class DailyReceivingsHandlerTests
    {
        private readonly Mock<ILogger<DailyReceivingsHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<IOpenBankingService> _openBankingService;

        public DailyReceivingsHandlerTests()
        {
            _mockLogger = new Mock<ILogger<DailyReceivingsHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _openBankingService = new Mock<IOpenBankingService>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenCustomerInfoReturnsTrueWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var dailyReceivingsHandler = new DailyReceivingsHandler(_mockLogger.Object, _mockFlagHelper.Object, _openBankingService.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1
                }
            };

            var mockDaily = new DailyReceivingsDTO()
            {
                Data = true,
                Success = true
            };

            var expected = FlagResultEnum.PendingApproval;

            _openBankingService.Setup(mock => mock.GetDailyReceivings(1)).ReturnsAsync(mockDaily);

            // Act
            var actual = dailyReceivingsHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenCustomerInfoReturnsFalseWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var dailyReceivingsHandler = new DailyReceivingsHandler(_mockLogger.Object, _mockFlagHelper.Object, _openBankingService.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1
                }
            };

            var mockDaily = new DailyReceivingsDTO()
            {
                Data = false,
                Success = true
            };

            var expected = FlagResultEnum.Processed;

            _openBankingService.Setup(mock => mock.GetDailyReceivings(1)).ReturnsAsync(mockDaily);

            // Act
            var actual = dailyReceivingsHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}