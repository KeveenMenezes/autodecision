using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using AutodecisionMultipleFlagsProcessor.Utility;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using DebitCard = AutodecisionCore.Contracts.ViewModels.Application.DebitCard;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class EmploymentLengthHandlerTests
    {
        private readonly Mock<ILogger<EmploymentLengthHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private EmploymentLengthHandler _employmentLengthHandler;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;

        public EmploymentLengthHandlerTests()
        {
            _mockLogger = new Mock<ILogger<EmploymentLengthHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();
            _employmentLengthHandler = new EmploymentLengthHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object);

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, false);
        }

        [Fact(DisplayName = "When Length Of Employment is null - Raise as Ignored")]
        public void WhenMinLengthOfEmploymentIsNullShouldReturnsPendingApprovalStatus()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddDays(-20)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LoeNew = null
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true,
                    CustomerSkipAutoDeny = new CustomerSkipAutoDeny()
                    {
                        Active = true,
                        ActivatedAt = DateTime.Now,
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            IsActive = true
                        }
                    }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Theory(DisplayName = "When Hire Date is less than LOE But CanValidateOpenPayrollInconsistency is False - Raise as Pending Approval")]
        [InlineData(false, BMGMoneyProgram.LoansForAll, ProductId.Standard, ApplicationTypeLocal.NewLoan, true)]
        [InlineData(true, BMGMoneyProgram.LoansAtWork, ProductId.Standard, ApplicationTypeLocal.NewLoan, true)]
        [InlineData(true, BMGMoneyProgram.LoansForAll, ProductId.Cashless, ApplicationTypeLocal.NewLoan, true)]
        [InlineData(true, BMGMoneyProgram.LoansForAll, ProductId.Standard, ApplicationTypeLocal.Refi, true)]
        [InlineData(true, BMGMoneyProgram.LoansForAll, ProductId.Standard, ApplicationTypeLocal.NewLoan, false)]
        public void WhenHireDateIsLessThanMinLengthOfEmploymentAndCanValidateOpenPayrollInconsistencyIsFalseShouldReturnsPendingApprovalStatus(
            bool employerAllowAutoDeny, string program, int product, string type, bool openPayrollConnect)
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = program,
                    Status = "2",
                    Type = type,
                    EmployerId = 123,
                    ProductId = product,
                    VerifiedDateOfHire = DateTime.Now.AddMonths(-1)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LoeNew = type == ApplicationType.NewLoan ? 600 : null,
                        LoeRefi = type == ApplicationType.Refi ? 600 : null,
                    },
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>()
                       {
                           new EmployerRulesItem()
                           {
                               Key = "2",
                               Required = true
                           },
                           new EmployerRulesItem()
                           {
                               Key = "8",
                               Required = true
                           }
                       }
                    }
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "DONE",
                    Liveness = true,
                    DocumentScanSuccess = true,
                },
                DebitCard = new DebitCard()
                {
                    BinName = "BMG Money",
                    IsConnected = true
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = employerAllowAutoDeny
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = openPayrollConnect ? new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-1),
                            IsActive = true
                        }
                    } : new List<OpenPayrollConnection> { }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Theory(DisplayName = "When Hire Date is less than LOE But HasOpenPayrollInconsistency is True - Raise as Pending Approval")]
        [InlineData(698, 17, true, true)]
        [InlineData(698, 19, true, true)]
        [InlineData(698, 1, true, true)]
        [InlineData(2501, 15, true, true)]
        [InlineData(100, 15, false, true)]
        [InlineData(100, 15, false, false)]
        public void WhenHireDateIsLessThanMinLengthOfEmploymentAndHasOpenPayrollInconsistencyIsTrueShouldReturnsPendingApprovalStatus(
            int employerId, int numberOfMonth, bool hasVerifiedDateOfHire, bool openPayrollConnect)
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForAll,
                    Status = "2",
                    Type = ApplicationTypeLocal.NewLoan,
                    EmployerId = employerId,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = SetVerifiedDateOfHire(employerId, numberOfMonth, hasVerifiedDateOfHire)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = employerId,
                        LoeNew = (new DateTime(2021, 10, 20)).GetDifferenceInDaysFromToday(),
                    },
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>()
                       {
                           new EmployerRulesItem()
                           {
                               Key = "2",
                               Required = true
                           },
                           new EmployerRulesItem()
                           {
                               Key = "8",
                               Required = true
                           }
                       }
                    }
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "DONE",
                    Liveness = true,
                    DocumentScanSuccess = true,
                },
                DebitCard = new DebitCard()
                {
                    BinName = "BMG Money",
                    IsConnected = true
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = openPayrollConnect ? new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-2),
                            IsActive = true
                        }
                    } : new List<OpenPayrollConnection> { }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }


        [Fact(DisplayName = "When Hire Date is less than LOE but employer doesn't allow auto deny - Raise as Pending Approval")]
        public void WhenHireDateIsLessThanMinLengthOfEmploymentButEmployerDoesntAllowAutoDenyShouldReturnsPendingApprovalStatus()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddDays(-20)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LoeNew = 600
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = false
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            IsActive = true
                        }
                    }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact(DisplayName = "When Hire Date is less than LOE but customer skipped auto deny - Raise as Pending Approval")]
        public void WhenHireDateIsLessThanMinLengthOfEmploymentButCustomerSkippedAutoDenyShouldReturnsPendingApprovalStatus()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddDays(-20)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LoeNew = 600
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true,
                    CustomerSkipAutoDeny = new CustomerSkipAutoDeny()
                    {
                        Active = true,
                        ActivatedAt = DateTime.Now,
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            IsActive = true
                        }
                    }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact(DisplayName = "When Hire Date is less than LOE but customer skipped auto deny - Raise as AutoDeny")]
        public void WhenHireDateIsLessThanMinLengthOfEmploymentAndItsPossibleToAutoDenyShouldReturnsAutoDenyStatus()
        {

            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, true);

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    SubmittedAt = DateTime.Now.AddHours(-10),
                    FundingMethod = "debit_card",
                    VerifiedDateOfHire = DateTime.Now.AddDays(-20)
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LoeNew = 600
                    },
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>()
                       {
                           new EmployerRulesItem()
                           {
                               Key = "2",
                               Required = true
                           },
                           new EmployerRulesItem()
                           {
                               Key = "8",
                               Required = true
                           }
                       }
                    }
                },
                DebitCard = new DebitCard()
                {
                    BinName = "BMG Money",
                    IsConnected = true
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true,
                    CustomerSkipAutoDeny = new CustomerSkipAutoDeny()
                    {
                        Active = false,
                        ActivatedAt = DateTime.Now,
                    }
                },
                FaceRecognition = new FaceRecognition()
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "TESTE"
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            IsActive = true
                        }
                    }
                },
                OpenBanking = new OpenBanking()
                {
                    Connections = new List<OpenBankingConnections>()
                    {
                        new OpenBankingConnections()
                        {
                            Vendor = "A"
                        }
                    }
                }
            };

            //Act
            var response = _employmentLengthHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.AutoDeny, response.FlagResult);
        }

        private static DateTime? SetVerifiedDateOfHire(int employerId, int verifiedDateOfHire, bool hasVerifiedDateOfHire)
        {
            return hasVerifiedDateOfHire ? (employerId != 2501 ? DateTime.Today.AddMonths(-verifiedDateOfHire) : new DateTime(2021, 11, 20).Date) : null;
        }
    }
}