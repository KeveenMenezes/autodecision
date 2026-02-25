using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{

    public class OneLoanPerSSNHandlerTests
    {
        private readonly Mock<ILogger<OneLoanPerSSNHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        public OneLoanPerSSNHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OneLoanPerSSNHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenAnotherLoanIsOpen()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    CustomerId = 2,
                    Type = ApplicationType.NewLoan

                },
                WhiteList = new AutodecisionCore.Contracts.ViewModels.Application.WhiteList()
                {
                    Reason = "test"
                }
            };

            var otherOpenLoan = new OtherOpenLoanDTO
            {
                Data = "222"
            };

            var handler = new OneLoanPerSSNHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            _mockCustomerInfo.Setup(_ => _.GetOtherOpenLoan(obj.Application.Id, obj.Application.CustomerId, obj.Application.Type)).Returns(Task.FromResult(otherOpenLoan));

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenApplicationNotFoundOnWhiteListShouldDeny()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    CustomerId = 2,
                    Type = ApplicationType.NewLoan
                   
                }
            };

            var otherOpenLoan = new OtherOpenLoanDTO
            {
                Data = "222"
            };

            var handler = new OneLoanPerSSNHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            _mockCustomerInfo.Setup(_ => _.GetOtherOpenLoan(obj.Application.Id, obj.Application.CustomerId, obj.Application.Type)).Returns(Task.FromResult(otherOpenLoan));

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.AutoDeny, resposta.FlagResult);
        }
    }
}
