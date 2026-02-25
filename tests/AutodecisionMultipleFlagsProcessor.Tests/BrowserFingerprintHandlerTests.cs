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
    public class BrowserFingerprintHandlerTests
    {
        private readonly Mock<ILogger<BrowserFringerprintHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public BrowserFingerprintHandlerTests()
        {
            _mockLogger = new Mock<ILogger<BrowserFringerprintHandler>>();
            _mockFlagHelper =  new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo> ();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenCustomerHasOtherApplicationsShouldRaiseFlag()
        {

            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {  
                    LoanNumber = "000002",
                    BrowserFingerprint = "5ab9814fadbb5552f17d14b0869d401f"

                }
            };
            ApplicationDto application = new ApplicationDto() { LoanNumber = "000001" };
            var applications = new List<ApplicationDto>() { application };

            _mockCustomerInfo.Setup(x => x.GetOtherApplicationsWithSameFingerprint("5ab9814fadbb5552f17d14b0869d401f", 413667)).ReturnsAsync(applications);

            var handler = new BrowserFringerprintHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);

        }
        [Fact]
        public void WhenCustomerDoesntHaveOtherApplicationsShouldReturnProcessed()
        {

            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {
                    LoanNumber = "000002",
                    BrowserFingerprint = "5ab9814fadbb5552f17d14b0869d401f"

                }
            };
           
            _mockCustomerInfo.Setup(x => x.GetOtherApplicationsWithSameFingerprint("5ab9814fadbb5552f17d14b0869d401f", 413667)).ReturnsAsync(new List<ApplicationDto>());

            var handler = new BrowserFringerprintHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);

        }

    }
}
