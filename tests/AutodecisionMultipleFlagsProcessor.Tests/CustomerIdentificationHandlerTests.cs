using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Google.Rpc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class CustomerIdentificationHandlerTests
    {
        private readonly Mock<ILogger<CustomerIdentificationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private CustomerIdentificationHandler _customerIdentificationHandler;

        public CustomerIdentificationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<CustomerIdentificationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _customerIdentificationHandler = new CustomerIdentificationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggle.Object);
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggle, false);
        }

        [Theory()]
        [InlineData("PENDING", "DONE", true, true, true)]
        [InlineData("DONE", "PENDING", true, true, true)]
        [InlineData("DONE", "DONE", false, true, true)]
        [InlineData("DONE", "PENDING", false, false, true)]
        [InlineData("DONE", "DONE", false, false, true)]
        public void WhenCustomerFaceRecognitionEnrollmentStatusIsntDone(string enrollmentStatus, string fraudStatus, bool documentScanSuccess, bool liveness, bool hasMoreThanOneClientId)
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "111"

                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication {
                        HasCustomerIdentiyValidated = false,
                        ReconciliationSystem = "test",
                        CreatedAt = DateTime.Now
                    }
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = enrollmentStatus,
                    FraudStatus = fraudStatus,
                    Liveness = liveness,
                    DocumentScanSuccess = documentScanSuccess,
                    DocumentData = new DocumentDataScanned()
                    {
                        FirstName = "Trinity",
                        LastName = "Monroe"
                    },
                    ClientIdsMatch = hasMoreThanOneClientId ? new List<int>() : new List<int>() { 1, 2 }
                },
                CreditPolicy = new CreditPolicy()
                {
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                         {
                             new EmployerRulesItem() {
                                Key = "2",
                                Required = true,
                             }
                         }
                    }
                }
            };
            var response = _customerIdentificationHandler.ProcessFlag(obj);
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Fact]
        public void WhenCustomerFaceRecognitionEnrollmentStatusIsDoneAndLivenessAndDocumentScanSuccess()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "111"
                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication {
                        HasCustomerIdentiyValidated = false,
                        ReconciliationSystem = "test",
                        CreatedAt = DateTime.Now
                    }
                },
                Customer = new Customer
                {
                    FirstName = "Homer",
                    LastName = "Bandeira"
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "DONE",
                    Liveness = true,
                    DocumentScanSuccess = true,
                    DocumentData = new DocumentDataScanned()
                    {
                        FirstName = "Homer J.",
                        LastName = "Bandeira"
                    }
                },
                CreditPolicy = new CreditPolicy()
                {
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                         {
                             new EmployerRulesItem() {
                                Key = "2",
                                Required = true,
                             }
                         }
                    }
                }
            };
            var response = _customerIdentificationHandler.ProcessFlag(obj);
            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Fact(Skip = "true")]
        public void WhenCustomersNameDoesntMatchCustomersNameOnDocument()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "123"
                },
                Customer = new Customer
                {
                    FirstName = "Joseph",
                    LastName = "Addams"
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "DONE",
                    Liveness = true,
                    DocumentScanSuccess = true,
                    DocumentData = new DocumentDataScanned()
                    {
                        FirstName = "Trinity",
                        LastName = "Monroe"
                    }
                },
                CreditPolicy = new CreditPolicy()
                {
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                         {
                             new EmployerRulesItem() {
                                Key = "2",
                                Required = true,
                             }
                         }
                    }
                }
            };
            var response = _customerIdentificationHandler.ProcessFlag(obj);
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
        }

        [Theory()]
        [InlineData(ApplicationStatus.Booked)]
        [InlineData(ApplicationStatus.Liquidated)]
        public void WhenCustomersHadLastApplicationThatAlreadyApprovedCustomerIdentification(string status)
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    LoanNumber = "123"
                },
                Customer = new Customer
                {
                    FirstName = "Joseph",
                    LastName = "Addams"
                },
                LastApplications = new List<LastApplication>()
                {
                    new LastApplication {
                        HasCustomerIdentiyValidated = true,
                        Status = status,
                        ReconciliationSystem = "test",
                        CreatedAt = DateTime.Now
                    }
                },
                FaceRecognition = new FaceRecognition
                {
                    EnrollmentStatus = "DONE",
                    FraudStatus = "DONE",
                    Liveness = true,
                    DocumentScanSuccess = true,
                    DocumentData = new DocumentDataScanned()
                    {
                        FirstName = "Trinity",
                        LastName = "Monroe"
                    }
                },
                CreditPolicy = new CreditPolicy()
                {
                    EmployerRules = new EmployerRules()
                    {
                        EmployerRulesItems = new List<EmployerRulesItem>
                         {
                             new EmployerRulesItem() {
                                Key = "2",
                                Required = true,
                             }
                         }
                    }
                }
            };
            var response = _customerIdentificationHandler.ProcessFlag(obj);
            Assert.Equal(FlagResultEnum.Approved, response.FlagResult);
            Assert.Equal("Approved due to last application.", response.ApprovalNote);
        }
    }
}