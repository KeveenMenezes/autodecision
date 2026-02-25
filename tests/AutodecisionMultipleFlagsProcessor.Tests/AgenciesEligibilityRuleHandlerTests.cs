using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class AgenciesEligibilityRuleHandlerTests
    {
        private readonly Mock<ILogger<AgenciesEligibilityRuleHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public AgenciesEligibilityRuleHandlerTests()
        {
            _mockLogger = new Mock<ILogger<AgenciesEligibilityRuleHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationEmployerIsntCensusEligibleProcessingShouldBeIgnored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = false
                }
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenEmployerCensusIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = null,
                WhiteList = new WhiteList()
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenEmployerCensusWithCriteriaIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = new Census()
                {
                    FlagAgenciesEligibilityRuleValue = "and employer_id = 81 and rtrim(time_type) = 'Full Time' and rtrim(payment_method) in ('Salaried') "
                },
                WhiteList = new WhiteList()
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.NotFound
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenEmployerCensusCriteriaHaveIncorrectSyntax()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = new Census()
                {
                    CustomerId = 1,
                    FlagAgenciesEligibilityRuleValue = "aand employer_id = 81 and rtrim(time_type) = 'Full Time' and rtrim(payment_method) in ('Salaried','OPS') "
                    
                },
                WhiteList = new WhiteList()
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.Error
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Error, response.FlagResult);
        }

        [Fact]
        public void WhenEmployerCensusWithCriteriaAndWhiteListIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = new Census()
                {
                    CustomerId = 1,
                    FlagAgenciesEligibilityRuleValue = "and employer_id = 81 and rtrim(time_type) = 'Full Time' and rtrim(payment_method) in ('Salaried', 'OPS') "
                },
                WhiteList = null
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.NotFound
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.AutoDeny, response.FlagResult);
        }

        [Fact]
        public void WhenFlagAgenciesEligibilityRuleValueIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 2,
                    CustomerId = 4
                },
                Census = new Census()
                {
                    FlagAgenciesEligibilityRuleValue = null
                },
            };

            var handler = new AgenciesEligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }
    }
}