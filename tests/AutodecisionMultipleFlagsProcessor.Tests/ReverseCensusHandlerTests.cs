using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class ReverseCensusHandlerTests
    {
        private readonly Mock<ILogger<ReverseCensusHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public ReverseCensusHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ReverseCensusHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationStatusIsDifferentOfProcessing()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "1"
                }
            };

            var handler = new ReverseCensusHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                }
            };

            var handler = new ReverseCensusHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusIsnotNullAndFlagReverseCensusIsActive()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census()
                {
                    FlagReverseCensusActive = true
                }
            };

            var handler = new ReverseCensusHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusIsnotNullAndFlagReverseCensusIsnotActive()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census()
                {
                    FlagReverseCensusActive = false
                }
            };

            var handler = new ReverseCensusHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }
    }
}