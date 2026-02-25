using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using AutodecisionCore.Contracts.Constants;
using Moq;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class PhoneVoipHandlerTests
    {
        private readonly Mock<ILogger<PhoneVoipHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        public PhoneVoipHandlerTests()
        {
            _mockLogger = new Mock<ILogger<PhoneVoipHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationTypeIsntNewLoanShouldBeIgnored()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    Type = ApplicationType.Refi
                }
            };

            var handler = new PhoneVoipHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public void WhenCustomerMobileNetworkTypeIsVoipShouldRaiseFlag()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Customer = new AutodecisionCore.Contracts.ViewModels.Application.Customer()
                {
                    MobileNetworkType = MobileNetworkType.Voip
                },
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    Type = ApplicationType.NewLoan
                },
                WhiteList = new AutodecisionCore.Contracts.ViewModels.Application.WhiteList()
                {
                    Reason = "test"
                }

            };

            var handler = new PhoneVoipHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenMobileNetworkTypeIsVoipAndApplicationNotFoundOnWhiteListAndTurndownActiveShouldDeny()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Customer = new AutodecisionCore.Contracts.ViewModels.Application.Customer()
                {
                    MobileNetworkType = MobileNetworkType.Voip
                },
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    Type = ApplicationType.NewLoan,
                    TurndownActive = true
                }
            };

            var handler = new PhoneVoipHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.AutoDeny, resposta.FlagResult);
        }

        [Fact]
        public void WhenMobileNetworkTypeIsVoipAndApplicationNotFoundOnWhiteListAndTurnDownInactiveShouldPendingAproval()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Customer = new AutodecisionCore.Contracts.ViewModels.Application.Customer()
                {
                    MobileNetworkType = MobileNetworkType.Voip
                },
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    Type = ApplicationType.NewLoan,
                }
            };

            var handler = new PhoneVoipHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationDoesntRaiseFlagOrDoesntDenyApplicationShouldProccesed()
        {

            var obj = new AutodecisionCore.Contracts.ViewModels.Application.AutodecisionCompositeData()
            {
                Customer = new AutodecisionCore.Contracts.ViewModels.Application.Customer()
                {
                    MobileNetworkType = ""
                },
                Application = new AutodecisionCore.Contracts.ViewModels.Application.Application()
                {
                    Type = ApplicationType.NewLoan,
                },
                WhiteList = new AutodecisionCore.Contracts.ViewModels.Application.WhiteList()
                {
                    Reason = "test"
                }
            };

            var handler = new PhoneVoipHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, resposta.FlagResult);
        }


    }
}
