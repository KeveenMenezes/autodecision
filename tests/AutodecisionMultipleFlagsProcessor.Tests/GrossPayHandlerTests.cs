using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class GrossPayHandlerTests
    {
        private readonly Mock<ILogger<GrossPayHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public GrossPayHandlerTests()
        {
            _mockLogger = new Mock<ILogger<GrossPayHandler>>();
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

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
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

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusSalaryPerPeriodIsZeroOrNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census
                {
                    SalaryPerPeriod = 0,
                    FlagGrossPayValue = 0.99M
                }
            };

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenGrossPayRangeValueIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census
                {
                    SalaryPerPeriod = 10000,
                    FlagGrossPayValue = null
                }
            };

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenGrossPayRangeValueIsZero()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census
                {
                    SalaryPerPeriod = 10000,
                    FlagGrossPayValue = 0
                }
            };

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerSalaryIsBetweenMinAndMaxRange()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },
                Census = new Census
                {
                    SalaryPerPeriod = 10000,
                    FlagGrossPayValue = 0.99M
                },
                Customer = new Customer
                {
                    Salary = 20000
                }
            };

            var handler = new GrossPayHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }
    }
}