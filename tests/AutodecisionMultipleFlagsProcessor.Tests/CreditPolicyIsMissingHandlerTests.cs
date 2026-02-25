using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class CreditPolicyIsMissingHandlerTests
    {
        private readonly Mock<ILogger<CreditPolicyIsMissingHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public CreditPolicyIsMissingHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CreditPolicyIsMissingHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenCreditPolicyIsNotMissing()
        {
            var data = new AutodecisionCompositeData()
            {
                Application = new Application() { LoanNumber = "123456789" },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem { Key = "open_banking_mandatory", Required = true },
                            new EmployerRulesItem { Key = "open_payroll_mandatory", Required = true },
                            new EmployerRulesItem { Key = "length_of_employment", Required = true, Min = "180" },
                        }
                    }
                }
            };
            var handler = new CreditPolicyIsMissingHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(data);
            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenCreditPolicyIsMissing()
        {
            var data = new AutodecisionCompositeData()
            {
                Application = new Application() { LoanNumber = "123456789" },
                CreditPolicy = new CreditPolicy() { EmployerRules = new EmployerRules() { EmployerRulesItems = new List<EmployerRulesItem>() } }
            };
            var handler = new CreditPolicyIsMissingHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(data);
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }
    }
}