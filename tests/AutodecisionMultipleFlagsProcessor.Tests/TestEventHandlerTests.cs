using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using BMGMoney.SDK.V2.Cache.Redis;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class TestEventHandlerTests
    {

        //private readonly IRedisCacheService _redisCacheService;
        public TestEventHandlerTests()
        {
        }

        [Fact]
        public void WhenFoundOnBlockListWeShouoldReaiseTheFlag()
        {
            var mockLogger = new Mock<ILogger<TestEventHandler>>();
            var mockFlagHelper = new Mock<IFlagHelper>();

            // Arrange
            var xpto1 = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    
                },
                BlockList = new AutodecisionCore.Contracts.ViewModels.Application.BlockList()
                {
                    Reason = "test"
                }
            };

            // Act
            var handler = new TestEventHandler(mockLogger.Object, mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(xpto1);

            // Assert
            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);

        }
    }
}