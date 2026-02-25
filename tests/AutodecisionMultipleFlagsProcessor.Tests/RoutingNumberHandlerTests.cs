using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class RoutingNumberHandlerTests
    {
        private readonly Mock<ILogger<RoutingNumberHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public RoutingNumberHandlerTests()
        {
            _mockLogger = new Mock<ILogger<RoutingNumberHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberIsNullOrEmptyShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = null
                }
            };

            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberIsEmptyShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = ""
                }
            };

            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberIsntNineNumericDigitsShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = "1234567"
                }
            };

            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationBankRoutingNumberCalculateCheckSumReturnFalseShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = "011401535"
                }
            };

            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }
        [Fact]
        public void WhenApplicationBankRoutingNumberDoesntExistsShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = "011401533"
                }
            };
            _mockCustomerInfo.Setup(x => x.CheckIfRoutingNumberAlreadyExists("011401533")).ReturnsAsync(false);
            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationBankRoutingIsCorrectSouldProccessFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    BankRoutingNumber = "011401533"
                }
            };
            _mockCustomerInfo.Setup(x => x.CheckIfRoutingNumberAlreadyExists("011401533")).ReturnsAsync(true);
            var handler = new RoutingNumberHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, resposta.FlagResult);
        }
    }
}
