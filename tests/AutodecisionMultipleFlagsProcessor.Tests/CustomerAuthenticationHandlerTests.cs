using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class CustomerAuthenticationHandlerTests
    {

        private readonly Mock<ILogger<CustomerAuthenticationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public CustomerAuthenticationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CustomerAuthenticationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenCensusIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Census = null,
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                }
            };

            var handler = new CustomerAuthenticationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerSimilarityRangeValueIsNull()
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
                    FlagCustomerSimilarityValue = null
                }
            };

            var handler = new CustomerAuthenticationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerSimilarityRangeValueIsZero()
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
                    FlagCustomerSimilarityValue = 0
                }
            };

            var handler = new CustomerAuthenticationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCensusAndCustomerDataIsEqual()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    FirstName = "Teste Nome",
                    LastName = "Teste Sobrenome",
                    EmployeeRegistration = "111",
                    DateOfBirth = new DateTime(1988, 06, 12)
                },
                Census = new Census
                {
                    CustomerId = 1,
                    FirstName = "Teste Nome",
                    LastName = "Teste Sobrenome",
                    EmployeeRegistration = "111",
                    DateOfBirth = new DateTime(1988, 06, 12),
                    FlagCustomerSimilarityValue = 0.99M
                },
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },

            };

            var handler = new CustomerAuthenticationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenCensusAndCustomerDataIsDifferent()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    FirstName = "Teste Nom",
                    LastName = "Teste Sobrenom",
                    EmployeeRegistration = "1112",
                    DateOfBirth = new DateTime(1989, 06, 12)
                },
                Census = new Census
                {
                    FirstName = "Teste Nome",
                    LastName = "Teste Sobrenome",
                    EmployeeRegistration = "111",
                    DateOfBirth = new DateTime(1988, 06, 12),
                    FlagCustomerSimilarityValue = 0.99M
                },
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true
                },

            };

            var handler = new CustomerAuthenticationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }
    }
}