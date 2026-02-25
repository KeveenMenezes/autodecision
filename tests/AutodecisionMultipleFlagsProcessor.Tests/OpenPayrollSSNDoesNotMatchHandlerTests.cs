using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenPayrollSSNDoesNotMatchHandlerTests
    {
        private readonly Mock<ILogger<OpenPayrollSSNDoesNotMatchHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        public OpenPayrollSSNDoesNotMatchHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenPayrollSSNDoesNotMatchHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenOpenPayrollVendorSSNDoesntMatches()
        {
            var obj = new AutodecisionCompositeData()
            {
                OpenPayroll = new OpenPayroll
                {
                    Connections = new  List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            }
                        }
                    }
                },
                Customer = new Customer
                {
                    Ssn = "222211222"
                },
                Application = new Application
                {
                    LoanNumber = "1111"
                }
            };

            var handler = new OpenPayrollSSNDoesNotMatchHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

    }
}
