using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenPayrollInconsistencyHandlerTests
    {
        private readonly Mock<ILogger<OpenPayrollInconsistencyHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private OpenPayrollInconsistencyHandler _openPayrollInconsistencyHandler;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;


        public OpenPayrollInconsistencyHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenPayrollInconsistencyHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();

            _openPayrollInconsistencyHandler = new OpenPayrollInconsistencyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object);
        }

        [Theory]
        [InlineData(false, "2", 1, "1", "The application employer doesn't allow auto deny")]
        [InlineData(true, "1", 1, "1", "The application program is LoansAtWork")]
        [InlineData(true, "2", 1, "1", "Open Payroll not connected")]
        public void ValidateIgnoredSituations(bool employerAllowAutoDeny, string programAllowed, int productAllow, string applicationType, string messageExpected)
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = programAllowed,
                    Status = "3",
                    Type = applicationType,
                    ProductId = productAllow,
                    VerifiedDateOfHire = DateTime.Now
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = employerAllowAutoDeny
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = false
                        }
                    }
                }
            };
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("Flags253And255")).Returns(true);

            //Act
            var response = _openPayrollInconsistencyHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }


        [Theory]
        [InlineData(2501, "2021-11-20", "Asking for customer the oldest paystub to manual approval ")]
        [InlineData(2501, "2022-11-20", "The hire date is more recent than oldest pay date ")]
        public void ValidSpecificHireDate2501(int employerId, string hireDate, string messageExpected)
        {

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "2",
                    Status = "3",
                    Type = "1",
                    ProductId = 1,
                    VerifiedDateOfHire = DateTime.Parse(hireDate),
                    EmployerId = employerId
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = true
                        }
                    }
                }
            };
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("Flags253And255")).Returns(true);

            //Act
            var response = _openPayrollInconsistencyHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }


        [Theory]
        [InlineData(698, "TODAY_MINUS_17_MONTHS", "Asking for customer the oldest paystub to manual approval The hire date is more recent than oldest pay date ")]
        [InlineData(698, "TODAY_MINUS_19_MONTHS", "Asking for customer the oldest paystub to manual approval The hire date is more recent than oldest pay date ")]
        [InlineData(698, "TODAY_MINUS_18_MONTHS", "Asking for customer the oldest paystub to manual approval The hire date is more recent than oldest pay date ")]
        public void ValidSpecificHireDate698(int employerId, string hireDate, string messageExpected)
        {
            DateTime hireDateCalculate = new DateTime(0001, 01, 01).Date;

            if (hireDate == "TODAY_MINUS_17_MONTHS")
                hireDateCalculate = DateTime.Now.AddMonths(-17);

            if (hireDate == "TODAY_MINUS_19_MONTHS")
                hireDateCalculate = DateTime.Now.AddMonths(-19);

            if (hireDate == "TODAY_MINUS_18_MONTHS")
                hireDateCalculate = DateTime.Now.AddMonths(-18);

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "2",
                    Status = "3",
                    Type = "1",
                    ProductId = 1,
                    VerifiedDateOfHire = hireDateCalculate,
                    EmployerId = employerId
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = new DateTime(2020, 11, 20).Date,
                            IsActive = true
                        }
                    }
                }
            };
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("Flags253And255")).Returns(true);

            //Act
            var response = _openPayrollInconsistencyHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }

        [Theory]
        [InlineData("2019-11-20", "2021-11-20", "The hire date is more recent than oldest pay date ", FlagResultEnum.PendingApproval)]
        [InlineData("2021-11-20", "2020-11-20", null, FlagResultEnum.Processed)]
        public void HireDateIsValidTowardsOldestPayDate(string oldestPayDate, string hireDate, string messageExpected, FlagResultEnum resultFlag)
        {

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "2",
                    Status = "3",
                    Type = "1",
                    ProductId = 1,
                    VerifiedDateOfHire = DateTime.Parse(hireDate),
                    EmployerId = 6
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Parse(oldestPayDate),
                            IsActive = true
                        }
                    }
                }
            };
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("Flags253And255")).Returns(true);

            //Act
            var response = _openPayrollInconsistencyHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(resultFlag, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }

        [Theory]
        [InlineData(null, "The hire date is null but there is a valid paystub ", FlagResultEnum.PendingApproval)]
        [InlineData("2020-11-20", "Open Payroll not connected", FlagResultEnum.Ignored)]
        public void HireDateNullWithValidPaystub(string hireDate, string messageExpected, FlagResultEnum resultFlag)
        {

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "2",
                    Status = "3",
                    Type = "1",
                    ProductId = 1,
                    VerifiedDateOfHire = new DateTime(2021, 11, 20).Date,
                    EmployerId = 6
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = true
                        }
                    }
                }
            };

            if (hireDate != null)
            {
                obj.OpenPayroll.Connections = new List<OpenPayrollConnection>();
                obj.Application.VerifiedDateOfHire = DateTime.Parse(hireDate);
            }
            else
            {
                obj.Application.VerifiedDateOfHire = null;

            }
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("Flags253And255")).Returns(true);

            //Act
            var response = _openPayrollInconsistencyHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(resultFlag, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }
    }
}