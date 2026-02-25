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
    public class OpenPayrollNotConnectedHandlerTests
    {
        private readonly Mock<ILogger<OpenPayrollNotConnectedHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        public OpenPayrollNotConnectedHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenPayrollNotConnectedHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
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
                            ConnectedAt = DateTime.Now.AddDays(-10)
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

            var handler = new OpenPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenPayrollAndIsZero()
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

            var handler = new OpenPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsRefiAndDaysConnectedWithOpenPayrollAndIsBiggerThanZero()
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
                            ConnectedAt = DateTime.Now.AddDays(20)
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

            var handler = new OpenPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
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
                            ConnectedAt = DateTime.Now.AddDays(-20)
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

            var handler = new OpenPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenApplicationTypeIsNewLoanAndOpenPayrollConnectedIsNotValidButThereIsAPaystubUploaded()
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
                        } }
                }
            };

            var handler = new OpenPayrollNotConnectedHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var response = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }
    }
}