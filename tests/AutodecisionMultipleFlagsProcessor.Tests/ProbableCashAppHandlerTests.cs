using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Extensions;
using System.Security.Policy;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class ProbableCashAppHandlerTests
    {
        private readonly Mock<ILogger<ProbableCashAppHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private ProbableCashAppHandler _probableCashAppHandler;

        public ProbableCashAppHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ProbableCashAppHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            _probableCashAppHandler = new ProbableCashAppHandler(_mockLogger.Object, _mockFlagHelper.Object);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberIsProbableCashShouldBePendingApproval()
        {
            var messageExpected = $"The bank routing number 041215663 comes from a CashApp";
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    IsProbableCashApp = true,
                    BankRoutingNumber = "041215663"
                }
            };

            var response = _probableCashAppHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberIsNotProbableCashShouldBeProcessed()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    BankRoutingNumber = "111111111"
                }
            };

            var response = _probableCashAppHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
            Assert.Null(response.Message);
        }
    }
}
