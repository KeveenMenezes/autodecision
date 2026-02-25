using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenBankingPayrollNotConnectedHandlerTests
    {
        private readonly Mock<ILogger<OpenBankingPayrollNotConnectedHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public OpenBankingPayrollNotConnectedHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenBankingPayrollNotConnectedHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenOpenBankingPayrollIsNullAndIsMandatory()
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
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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
                            Uploaded = false,
                        } }
                }
            };
            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenOpenBankingPayrollIsNullAndIsntMandatory()
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

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenPayrollIsBiggerThanZeroAndLessThanMaxConnectedDaysAccepted()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-6)
                        }
                    }
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
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
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
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-17)
                        }
                    }
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
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenBankingAndOpenPayrollIsZero()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now
                        }
                    }
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
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenBankingAndOpenPayrollIsBiggerThanMaxConnectedDaysAccepted()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-20)
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenBankingAndOpenPayroll()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.Refi
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Today.AddDays(-16)
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasConnectionIsntActiveAndEmployerHasRuleMandatory()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-20),
                            IsActive = false
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerJustConnectedOpenPayrollFlagShouldBeInored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-20),
                            IsActive = false
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections>
                    {
                    }
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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
                            Uploaded = true,
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerJustConnectedOpenBankingFlagShouldBeInored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>
                    {
                    }
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
                                Key = NewCreditPolicyRule.OpenPayrollMandatory,
                                Required = true
                            }
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasConnectionOpenPayrollAndOpenBanking()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-20)
                        }
                    }
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
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndHasPaystubDocumentUploaded()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now.AddDays(-20)
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem> {
                            new EmployerRulesItem
                            {
                                Key = NewCreditPolicyRule.OpenPayrollOrBankingMandatory,
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
                            Uploaded = true,
                            ReviewStatus = DocumentReviewStatus.Approved
                        }
                    }
                }
            };

            var handler = new OpenBankingPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }
    }
}