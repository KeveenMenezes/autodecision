using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class BankInfoFoundHandlerTests
    {

        private readonly Mock<ILogger<BankInfoFoundHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        public BankInfoFoundHandlerTests()
        {
            _mockLogger = new Mock<ILogger<BankInfoFoundHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicaitonWithSameBankInfoIsFound()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    CustomerId = 2,
                    BankRoutingNumber = "222",
                    BankAccountNumber = "333",
                    Status = "2"
                }
            };


            var applications = new ApplicationsWithSameBankInfoDTO()
            {

                Data = new List<ApplicationsWithSameBankInfo>(){
                    new ApplicationsWithSameBankInfo
                    {
                        BankRoutingNumber = "2221",
                        BankAccountNumber = "3331",
                        LoanNumber = "222"
                    }
                }

            };


            var handler = new BankInfoFoundHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            _mockCustomerInfo.Setup(_ => _.GetApplicationsWithSameBankInfo(obj.Application.CustomerId, obj.Application.BankRoutingNumber, obj.Application.BankAccountNumber)).Returns(Task.FromResult(applications));

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

    }
}
