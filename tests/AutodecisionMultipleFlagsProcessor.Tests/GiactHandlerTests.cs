using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.Constants;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using AutodecisionCore.Contracts.Enums;
using AutodecisionMultipleFlagsProcessor.DTOs;
using System.Threading;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Contracts.Messages;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class GiactHandlerTests
    {
        private readonly Mock<ILogger<GiactHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;

        public GiactHandlerTests()
        {
            _mockLogger = new Mock<ILogger<GiactHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, false);
        }

        [Fact]
        public async Task WhenProductIdIsCashlessShouldBeIgnored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    ProductId = ApplicationProductId.Cashless
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenProductIdIsCashlessAndDeferredPaymentShouldBeIgnored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    ProductId = ApplicationProductId.CashlessAndDeferredPayment
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenFundingMethodIsCheckAndApplicationTypeIsNewLoanShouldBeIgnored()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    FundingMethod = ApplicationFundingMethod.Check,
                    Type = ApplicationType.NewLoan
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenApplicationTypeIsRefiAndFtIsEnabledShouldBeIgnored()
        {
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, true);

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    Type = ApplicationType.Refi
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("bypass_giact_for_refi")).Returns(true);
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenFundingMethodIsCheckAndApplicationTypeIsRefiShouldRaiseFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    Type = ApplicationType.Refi,
                    FundingMethod = ApplicationFundingMethod.Check
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenGiactResultIsNullShouldRaiseFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    FundingMethod = ApplicationFundingMethod.Check
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var giactResult = new GiactResultDto();
            giactResult = null;

            _mockCustomerInfo.Setup(x => x.GetLastGiactResult("0001")).ReturnsAsync(giactResult);

            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenGiactResultAccountResponseStatusAndCustomerResponseStatusIsPassShouldProcessFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    FundingMethod = ApplicationFundingMethod.Check
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var giactResult = new GiactResultDto()
            {
                account_response_status = "pass",
                customer_response_status = "pass"
            };
            _mockCustomerInfo.Setup(x => x.GetLastGiactResult("0001")).ReturnsAsync(giactResult);
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenApplicationTypeIsRefiAndAccountResponseStatusIsntDeclineAndPreviousApplicationIsntNullAndBankRoutingNumberAndBankAccountNumberIsTheSameShouldApproveFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    Type = ApplicationType.Refi,
                    BankRoutingNumber = "000001",
                    BankAccountNumber = "000002"
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var giactResult = new GiactResultDto()
            {
                account_response_status = "tes",
                customer_response_status = "pass"
            };
            var previousApplication = new ApplicationDto()
            {
                BankAccountNumber = "000002",
                BankRoutingNumber = "000001",
                LoanNumber = "0002"
            };
            _mockCustomerInfo.Setup(x => x.GetLastGiactResult("0001")).ReturnsAsync(giactResult);
            _mockCustomerInfo.Setup(x => x.GetPreviousApplication(12)).Returns(previousApplication);

            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenAccountResponseStatusIsDeclineAndApplicationTypeIsntRefiShouldRaiseFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    Type = ApplicationType.NewLoan,
                    BankRoutingNumber = "000001",
                    BankAccountNumber = "000002"
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };
            var giactResult = new GiactResultDto()
            {
                account_response_status = "decline",
                customer_response_status = "pass"
            };
            var previousApplication = new ApplicationDto()
            {
                BankAccountNumber = "000001",
                BankRoutingNumber = "000002",
                LoanNumber = "0002"
            };
            _mockCustomerInfo.Setup(x => x.GetLastGiactResult("0001")).ReturnsAsync(giactResult);
            _mockCustomerInfo.Setup(x => x.GetPreviousApplication(001)).Returns(previousApplication);
            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public async Task WhenValidOpenBankingAccountAndvalidOpenPayrollRemainderAccounAreTrue()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "0001",
                    FundingMethod = ApplicationFundingMethod.Check
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenBanking = new OpenBanking
                {
                    Connections = new List<OpenBankingConnections> {
                        new OpenBankingConnections()
                        {
                            CreatedAt = DateTime.Now,
                            AccountNumber = "123456",
                            RoutingNumber = "0123456789",
                            IsDefault = true
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "*****3456",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "*******56789",
                                }
                            }
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
            .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
            {
                return (true, e);
            });

            _mockFlagHelper.Setup(x => x.ValidOpenPayrollRemainderAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
            .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
            {
                return (true, e);
            });

            var handler = new GiactHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object, _mockCustomerInfo.Object);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, resposta.FlagResult);
        }
    }
}
