using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class LevelOfCommitmentHandlerTest
    {
        private readonly Mock<ILogger<LevelOfCommitmentHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly LevelOfCommitmentHandler _levelOfCommitmentHandler;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;

        public LevelOfCommitmentHandlerTest()
        {
            _mockLogger = new Mock<ILogger<LevelOfCommitmentHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();

            _levelOfCommitmentHandler = new LevelOfCommitmentHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object);

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, true);

        }

        [Theory(DisplayName = "When Can not Validate LOC - Raised as Ignored")]
        [InlineData(false, "2", 3, "1", true, false)]
        [InlineData(true, "1", 2, "2", true, false)]
        public void WhenCannotValidatedLOCShouldReturnsIgnoredStatus(
            bool employerAllowAutoDeny,
            string program,
            int productType,
            string appType,
            bool isOpenPayrollValid,
            bool customerSkipAutoDeny)
        {

            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = program,
                    Status = "2",
                    Type = appType,
                    EmployerId = 123,
                    ProductId = productType,
                    VerifiedDateOfHire = DateTime.Now.AddMonths(-1),
                    VerifiedNetIncome = 110.0m,
                    AmountOfPayment = 1000,
                    FundingMethod = "debit_card"
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LocNew = 10.0m,
                        LocRefi = 10.0m,
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
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = employerAllowAutoDeny,
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.AditionalSourceIncomeDocument,
                            Uploaded = true,
                        } },
                    CustomerSkipAutoDeny = new CustomerSkipAutoDeny()
                    {
                        Active = customerSkipAutoDeny,
                        ActivatedAt = customerSkipAutoDeny ? DateTime.Now.AddDays(-15) : DateTime.Now,
                    },
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    BinName = null,
                    IsConnected = customerSkipAutoDeny
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
                OpenPayroll = new OpenPayroll
                {
                    Connections = isOpenPayrollValid ? new List<OpenPayrollConnection> {
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-1),
                            IsActive = true
                        }
                    } : new List<OpenPayrollConnection>()
                },
                TotalIncome = new TotalIncome()
                {
                    ApplicationId = 1,
                    TotalAmount = 1500,
                    StatusDescription = Enum.GetName(typeof(StatusIncome), StatusIncome.Approved),
                    PayFrequency = "Weekly",
                    Status = StatusIncome.Approved
                }
            };

            //Act
            var response = _levelOfCommitmentHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }


        [Theory(DisplayName = "When LOC is less then credit policy rule and has additional income - Raised as Processed")]
        [InlineData(1000, "1", "1", null, false, 100, false)]
        [InlineData(1000, "2", null, "1", true, 100, false)]
        [InlineData(1000, "1", "1", null, false, 100.0, true)]
        [InlineData(1000, "2", null, "1", true, 100.0, true)]
       
        public void WhenLOCIsValidShouldReturnsProcessedStatus(
            decimal netIncome,
            string applicationType,
            string? locNew,
            string? locRefi,
            bool hasPreviousAppId,
            decimal oldAmountOfPayment,
            bool hasAdditionalIncome)
        {
            //Arrange
            var lastApplications = new List<LastApplication>() {
                    new LastApplication()
                    {
                        Id = 10,
                        AmountOfPayment = oldAmountOfPayment,
                        ReconciliationSystem = "Test",
                        Status = "6"
                    }
            };

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    PreviousApplicationId = hasPreviousAppId ? lastApplications[0].Id : null,
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    AmountOfPayment = hasPreviousAppId ? lastApplications[0].AmountOfPayment + 50 : 100,
                    Status = "2",
                    Type = applicationType,
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddMonths(-1),
                    VerifiedNetIncome = netIncome
                },
                LastApplications = hasPreviousAppId ? lastApplications : null,
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LocNew = string.IsNullOrEmpty(locNew) ? null : 0.15M,
                        LocRefi = string.IsNullOrEmpty(locRefi) ? null : 0.15M,
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
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true,
                    ApplicationDocuments = new List<ApplicationDocuments> {
                        new ApplicationDocuments()
                        {
                            DocumentType = hasAdditionalIncome ? DocumentType.AditionalSourceIncomeDocument : "",
                            Uploaded = true
                        }
                    }
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    BinName = null,
                    IsConnected = false
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
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-1),
                            IsActive = true
                        }
                    }
                },
                TotalIncome = new TotalIncome()
                {
                    ApplicationId = 1,
                    TotalAmount = 1500,
                    StatusDescription = Enum.GetName(typeof(StatusIncome), StatusIncome.Approved),
                    PayFrequency = "Weekly",
                    Status = StatusIncome.Approved
                }
            };

            //Act
            var response = _levelOfCommitmentHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }


        [Theory(DisplayName = "When the netIncome and payment amount are valid or invalid, the return status is expected - Raised as Status Expected")]
        [InlineData(FlagResultEnum.PendingApproval,false, 100, 0, false, "NetIncome invalid. Value cannot be null or zero.")]
        [InlineData(FlagResultEnum.PendingApproval, true, 10, 100, false, "Amount of payment less than old payment from last application.")]
        [InlineData(FlagResultEnum.PendingApproval, false, 100, 10, true, "There is more than one employer on payout to this customer.")]
        [InlineData(FlagResultEnum.AutoDeny, false, 160, 1000, false, "Application denied by income. Application LOC: 16.00% | Credit Policy LOC: 10.00%")]
        [InlineData(FlagResultEnum.Processed, false, 80, 3000, false, "Flag processed successfully.")]
        public void WhenTheStatusReturnsExpectedMessageShouldBeExpected(FlagResultEnum flagReturn, bool hasPreviousAppId, decimal amountOfPayment, decimal netIncome, bool hasOneEmployer, string messageExpected)
        {
            //Arrange
            var lastApplications = new List<LastApplication>() {
                    new LastApplication()
                    {
                        Id = 10,
                        AmountOfPayment = hasPreviousAppId ? 100 : 0,
                        ReconciliationSystem = "Test",
                        Status = "6"
                    }
            };

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    PreviousApplicationId = hasPreviousAppId ? lastApplications[0].Id : null,
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    AmountOfPayment = hasPreviousAppId ? lastApplications[0].AmountOfPayment - 30 : amountOfPayment,
                    Status = "2",
                    Type = "1",
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddMonths(-1),
                    VerifiedNetIncome = netIncome
                },
                LastApplications = hasPreviousAppId ? lastApplications : null,
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LocNew = 0.10M,
                        LocRefi = 0.10M,
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
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    BinName = null,
                    IsConnected = false
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
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-1),
                            IsActive = true,
                            HasMoreThanOneEmployer = hasOneEmployer,
                        }
                    }
                },
                TotalIncome = new TotalIncome()
                {
                    ApplicationId = 1,
                    TotalAmount = netIncome,
                    StatusDescription = Enum.GetName(typeof(StatusIncome), StatusIncome.Approved),
                    PayFrequency = "Weekly",
                    Status = StatusIncome.Approved
                }
            };

        
            //Act
            var response = _levelOfCommitmentHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(flagReturn, response.FlagResult);
            Assert.Equal(messageExpected, response.Message);
        }

      

        [Fact(DisplayName = "When LOC is invalid for application - Raised AutoDenied")]
        public void WhenLOCIsInvalidShouldReturnsAutoDenyStatus()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    PreviousApplicationId = null,
                    LoanNumber = "123456789",
                    Program = BMGMoneyProgram.LoansForFeds,
                    AmountOfPayment = 100,
                    Status = "2",
                    Type = "1",
                    EmployerId = 123,
                    ProductId = ProductId.Standard,
                    VerifiedDateOfHire = DateTime.Now.AddMonths(-1),
                    VerifiedNetIncome = 900
                },
                CreditPolicy = new CreditPolicy()
                {
                    CreditPolicyEntity = new CreditPolicyEntity()
                    {
                        EmployerId = 123,
                        LocNew = 0.10M,
                        LocRefi = 0.10M,
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
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    EmployerAllowAutoDeny = true,
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    BinName = null,
                    IsConnected = false
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
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111"
                            },
                            OldestPayDate = DateTime.Now.AddMonths(-1),
                            IsActive = true,
                            HasMoreThanOneEmployer = false,
                        }
                    }
                },
                TotalIncome = new TotalIncome()
                {
                    ApplicationId = 1,
                    TotalAmount = 100,
                    StatusDescription = Enum.GetName(typeof(StatusIncome), StatusIncome.Approved),
                    PayFrequency = "Weekly",
                    Status = StatusIncome.Approved
                }
            };
          
            //Act
            var response = _levelOfCommitmentHandler.ProcessFlag(obj);

            //Assert
            Assert.Equal(FlagResultEnum.AutoDeny, response.FlagResult);
            Assert.Contains("Application denied by income.", response.Message);
        }
    }
}
