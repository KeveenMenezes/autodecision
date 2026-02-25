using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Extensions;

namespace AutodecisionMultipleFlagsProcessor.Tests
{

    public class EligibilityRuleHandlerTests
    {
        private readonly Mock<ILogger<EligibilityRuleHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        public EligibilityRuleHandlerTests()
        {
            _mockLogger = new Mock<ILogger<EligibilityRuleHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationStatusIsDifferentOfProcessing()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "1"
                }
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
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

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }


        [Fact]
        public void WhenCustomerCensusIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    ProductId = 3,
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = null
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
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
                    EmployerId = 47,
                    CustomerId = 1232
                },
                Census = new Census()
                {
                    FlagEligibilityRuleValue = "and employer_id = 47 and active_status = '3' and time_type in ('1','2') and payment_method in ('01','02') "
                },
                WhiteList = new WhiteList()
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.NotFound
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenEmployerCensusWithCriteriaWasFound()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Id = 1,
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 47,
                    CustomerId = 123
                },
                Census = new Census()
                {
                    CustomerId = 1,
                    FlagEligibilityRuleValue = "and employer_id = 47 and active_status = '3' and time_type in ('1','2') and payment_method in ('01','02') "
                },
                WhiteList = new WhiteList()
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = new CensusEmployer(),
                TransactionStatus = CensusTransactionStatus.Found
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenFlagEligibilityRuleValueIsEmpty()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                Census = new Census
                {
                    EmployerId = 2,
                    PayrollGroup = "ABC",
                    FlagEligibilityRuleValue = ""
                }
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusWithCriteriaIsNullAndWhiteListIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    ProductId = 3,
                    IsEmployerCensusEligible = true,
                    EmployerId = 81,
                    CustomerId = 2
                },
                WhiteList = null
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.NotFound
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusWithCriteriaIsNotNullAndTimeTypeEqualsProbationaryForEmployerId48()
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
                    EmployerId = 48,
                    TimeType = "Probationary",
                    FlagEligibilityRuleValue = "test"
                }
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = new CensusEmployer(),
                TransactionStatus = CensusTransactionStatus.Found
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, It.IsAny<string>())).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerCensusWithCriteriaIsNotNullAndPayrollGroupEqualsAB()
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
                    EmployerId = 124,
                    PayrollGroup = "AB",
                    FlagEligibilityRuleValue = "test"
                }
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = new CensusEmployer(),
                TransactionStatus = CensusTransactionStatus.Found
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
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
                    FlagEligibilityRuleValue = "and employer_id = 56 and personnel_sub_area = 'SCP' and payment_method = 'S' and active_status in ('A','P')"
                },
                WhiteList = new WhiteList()
            };

            var censusCriteriaDTO = new CensusDTO
            {
                Data = null,
                TransactionStatus = CensusTransactionStatus.Error
            };

            var handler = new EligibilityRuleHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            _mockCustomerInfo.Setup(_ => _.GetCensusByCustomerIdWithCriteria(obj.Application.EmployerId, obj.Application.CustomerId, obj.Census.FlagEligibilityRuleValue)).Returns(Task.FromResult(censusCriteriaDTO));
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Error, response.FlagResult);
        }
    }
}