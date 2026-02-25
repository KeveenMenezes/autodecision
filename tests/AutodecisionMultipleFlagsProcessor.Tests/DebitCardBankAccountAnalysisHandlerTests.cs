using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using AutodecisionMultipleFlagsProcessor.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class DebitCardBankAccountAnalysisHandlerTests
    {
        private readonly Mock<ILogger<DebitCardBankAccountAnalysisHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public DebitCardBankAccountAnalysisHandlerTests()
        {
            _mockLogger = new Mock<ILogger<DebitCardBankAccountAnalysisHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenApplicationFundingMethodAndPaymentTypeIsntDebitCardAndOpenBankingIsNotRequired()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "Teste",
                    PaymentType = "Teste"
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
                                Required = false
                            }
                        }
                    }
                }
            };

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationFundingMethodAndPaymentTypeIsntDebitCard()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "Teste",
                    PaymentType = "Teste"
                },
                DebitCard = null,
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
                            }
                        }
                    }
                }
            };

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsNullAndOpenBankingIsNotRequired()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "123"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = null,
                    IsConnected = false
                },
                Customer = new Customer(),
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = false
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsNullAndOpenPayrollIsNotRequired()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = null,
                    IsConnected = false
                },
                Customer = new Customer(),
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = false
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsNullAndOnlyOpenBankingIsRequiredButIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = null,
                    IsConnected = true
                },
                Customer = new Customer(),
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
                            }
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
            .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
            {
                return (false, new ProcessFlagResponseEvent
                {
                    Message = "test",
                    FlagResult = FlagResultEnum.PendingApproval
                });
            });

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsNullAndOnlyOpenPayrollIsRequiredButIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = null,
                    IsConnected = true
                },
                Customer = new Customer(),
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsDifferentOfBankNameAndOpenBankingAndOpenPayrollIsRequiredButNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste1",
                    IsConnected = false
                },
                CreditPolicy = new CreditPolicy
                {
                    EmployerRules = new EmployerRules
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                        {
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_banking_mandatory",
                                Required = true
                            }
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
          .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
          {
              return (false, new ProcessFlagResponseEvent
              {
                  Message = "test",
                  FlagResult = FlagResultEnum.PendingApproval
              });
          });

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenAccountsCustomerCardBinEmissorIsDifferentOfBankNameAndOpenBankingAndOpenPayrollIsNotRequired()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste1",
                    IsConnected = false
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = false
                            }
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
              .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
              {
                  return (false, new ProcessFlagResponseEvent
                  {
                      Message = "test",
                      FlagResult = FlagResultEnum.PendingApproval
                  });
              });

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameMatchesButOpenPayrollIsRequiredAndRemainderBankDetailsDoesNotMatchTheCustomer()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "12345",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "0012345",
                                }
                            }
                        }
                    }
                },
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameMatchesButOpenPayrollDoesNotHaveARemainderAccount()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = false,
                                    AccountNumber = "12345",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "amount",
                                    RoutingNumber = "0012345",
                                    Value = 2000
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameAndOpenPayrollRemainderBankDetailsMatchesTheCustomer()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "BMG Money"
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameMatchesButOpenBankingDoesNotMatchTheCustomer()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true
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
                                Required = false
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
                            AccountNumber = "00456",
                            RoutingNumber = "5678901234",
                            IsDefault = true
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
            .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
            {
                return (false, new ProcessFlagResponseEvent
                {
                    Message = "test",
                    FlagResult = FlagResultEnum.PendingApproval
                });
            });

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameMatchesButOpenBankingDoesNotHaveADefaultAccount()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true
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
                                Required = false
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
                            AccountNumber = "00456",
                            RoutingNumber = "5678901234",
                            IsDefault = false
                        }
                    }
                }
            };

            _mockFlagHelper.Setup(x => x.ValidOpenBankingAccount(It.IsAny<AutodecisionCompositeData>(), It.IsAny<ProcessFlagResponseEvent>(), It.IsAny<bool>()))
              .Returns((AutodecisionCompositeData x, ProcessFlagResponseEvent e, bool r) =>
              {
                  return (false, new ProcessFlagResponseEvent
                  {
                      Message = "test",
                      FlagResult = FlagResultEnum.PendingApproval
                  });
              });

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameAndOpenBankingMatchesTheCustomer()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "BMG Money"
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
                                Required = false
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameOpenBankingAndOpenPayrollRemainderBankDetailsMatchesTheCustomer()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "111",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "BMG Money"
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameOpenBankingAndOpenPayrollRemainderBankDetailsMatchesTheCustomerButNoPreviousLoanIsBooked()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "Teste"
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "12345",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "0012345",
                                }
                            }
                        }
                    }
                },
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameOpenBankingAndOpenPayrollRemainderBankDetailsMatchesTheCustomerButDebitCardIdIsNull()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 0,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "Teste"
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "12345",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "0012345",
                                }
                            }
                        }
                    }
                },
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenDebitCardBankNameOpenBankingAndOpenPayrollRemainderBankDetailsMatchesTheCustomerAndSameDebitCard()
        {
            var lastApplications = new List<LastApplication>() {
                new LastApplication()
                {
                    AmountOfPayment = 10.00M,
                    ReconciliationSystem = "Test",
                    Status = "6",
                    ApplicationConnections = new ApplicationConnections()
                    { 
                        DebitCardId = 123
                    }
                }
            };

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    FundingMethod = "debit_card",
                    PaymentType = "debit_card",
                    LoanNumber = "12345",
                    BankAccountNumber = "123456",
                    BankRoutingNumber = "0123456789"
                },
                Customer = new Customer
                {
                    BankName = "Teste"
                },
                DebitCard = new AutodecisionCore.Contracts.ViewModels.Application.DebitCard()
                {
                    Id = 123,
                    BinName = "Teste",
                    IsConnected = true,
                    CardBinEmissor = "Teste"
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
                                Required = false
                            },
                            new EmployerRulesItem()
                            {
                                Key = "open_payroll_mandatory",
                                Required = true
                            }
                        }
                    }
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            PayAllocations = new List<PayAllocations>()
                            {
                                new PayAllocations()
                                {
                                    IsRemainder = true,
                                    AccountNumber = "12345",
                                    CreatedAt = DateTime.Now,
                                    AccountType = "remainder",
                                    RoutingNumber = "0012345",
                                }
                            }
                        }
                    }
                },
                LastApplications = lastApplications
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

            var handler = new DebitCardBankAccountAnalysisHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Approved, response.FlagResult);
        }
    }
}