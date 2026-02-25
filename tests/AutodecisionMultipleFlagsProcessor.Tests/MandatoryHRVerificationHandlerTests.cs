using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;


namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class MandatoryHRVerificationHandlerTests
    {
        private readonly Mock<ILogger<MandatoryHRVerificationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public MandatoryHRVerificationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<MandatoryHRVerificationHandler>>();
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

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
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

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCensusValidationIsInactiveAndFlagMandatoryHRVerificationIsNecessary()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true, 
                    Program = "1"
                },
                Census = new Census()
                {
                    CensusValidation = false
                }
            };

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCensusValidationIsActiveAndFlagMandatoryHRVerificationIsNotNecessary()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    Program = "1"
                },
                Census = new Census()
                {
                    CensusValidation = true
                }
            };

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCensusValidationIsActiveAndApplicationProgramIsLFA()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    Program = "4"
                },
                Census = new Census()
                {
                    CensusValidation = true
                }
            };

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCensusValidationIsInactiveAndApplicationProgramIsLFA()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    Program = "4"
                },
                Census = new Census()
                {
                    CensusValidation = false
                }
            };

            var handler = new MandatoryHRVerificationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }
    }
}
