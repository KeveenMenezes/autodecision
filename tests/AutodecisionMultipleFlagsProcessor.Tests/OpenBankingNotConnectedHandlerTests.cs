using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenBankingNotConnectedHandlerTests
    {
        private readonly Mock<ILogger<OpenBankingNotConnectedHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public OpenBankingNotConnectedHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenBankingNotConnectedHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenOpenPayrollIsNullAndIsMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenBanking = null,
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.Paystub,
                            Uploaded = true
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenOpenPayrollIsNullAndIsntMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenPayroll = null,
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> { }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenOpenBankingIsNullAndIsMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenBanking = null,
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.Paystub,
                            Uploaded = true
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenOpenBankingIsNullAndIsntMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenBanking = null,
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>()
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenBankingIsBiggerThanZeroAndLessThanMaxConnectedDaysAccepted()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now.AddDays(-10)
                        }
                    }
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenBankingAndIsZero()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
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
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasConnectionIsntActiveAndEmployerHasRuleMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenBanking = new OpenBanking
                { },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.Paystub,
                            Uploaded = true
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasConnectionWithOpenBanking()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now.AddDays(-20)
                        }
                    }
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.Paystub,
                            Uploaded = true
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasBankStatementDocumentUploaded()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenBanking = new OpenBanking
                { },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenBankingMandatory,
                                Required = true
                            }
                        }
                    }
                },
                FlagValidatorHelper = new FlagValidatorHelper()
                {
                    ApplicationDocuments = new List<ApplicationDocuments>()
                    {
                        new ApplicationDocuments()
                        {
                            DocumentType = DocumentType.BankStatement,
                            Uploaded = true,
                            ReviewStatus = DocumentReviewStatus.Approved
                        }
                    }
                }
            };

            var handler = new OpenBankingNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }
    }
}