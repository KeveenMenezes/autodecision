using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers.Ocrolus;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenPayrollOcrolusDocumentStatusHandlerTests
    {
        private readonly Mock<ILogger<OpenPayrollOcrolusDocumentStatusHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly OpenPayrollOcrolusDocumentStatusHandler _openPayrollOcrolusDocStatusHandler;

        public OpenPayrollOcrolusDocumentStatusHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenPayrollOcrolusDocumentStatusHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            _openPayrollOcrolusDocStatusHandler = new OpenPayrollOcrolusDocumentStatusHandler(_mockLogger.Object, _mockFlagHelper.Object);
        }

        [Fact]
        public void ValidateIgnoredNoConnection()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "1",
                    Status = "3",
                    Type = "2",
                    ProductId = 3,
                    VerifiedDateOfHire = DateTime.Now,
                    EmployerName = "WALMART"
                },
                Customer = new Customer()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Ssn = "123456789"
                }
            };
            var response = _openPayrollOcrolusDocStatusHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void ValidateIgnoredNoActiveConnection()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "1",
                    Status = "3",
                    Type = "2",
                    ProductId = 3,
                    VerifiedDateOfHire = DateTime.Now,
                    EmployerName = "WALMART"
                },
                Customer = new Customer()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Ssn = "123456789"
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "22221111",
                                Name = "VITOR TRAMONTINA",
                                EmployerName = "WALMART"
                            },
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = false,
                            ConnectedAt = DateTime.Now,
                        }
                    }
                }
            };
            var response = _openPayrollOcrolusDocStatusHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void ValidateIgnoreWrongVendor()
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "1",
                    Status = "3",
                    Type = "2",
                    ProductId = 3,
                    VerifiedDateOfHire = DateTime.Now,
                    EmployerName = "WALMART"
                },
                Customer = new Customer()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Ssn = "123456789"
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = true,
                            ConnectedAt = DateTime.Now,
                            VendorType = OpenPayrollVendorConstant.Truv
                        }
                    }
                }
            };
            var response = _openPayrollOcrolusDocStatusHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Theory]
        [InlineData(OcrolusDocumentStatus.Verified, FlagResultEnum.Processed)]
        [InlineData(OcrolusDocumentStatus.SignalFound, FlagResultEnum.PendingApproval)]
        [InlineData(OcrolusDocumentStatus.Reject, FlagResultEnum.PendingApproval)]
        [InlineData(OcrolusDocumentStatus.Pending, FlagResultEnum.PendingApproval)]
        [InlineData(null, FlagResultEnum.PendingApproval)]
        public void ValidateResultFlag(int? documentStatus, FlagResultEnum resultStatus)
        {
            //Arrange
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "1",
                    Status = "3",
                    Type = "2",
                    ProductId = 3,
                    VerifiedDateOfHire = DateTime.Now,
                    EmployerName = "WALMART"
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            IsActive = true,
                            ConnectedAt = DateTime.Now,
                            OcrolusDocumentStatus = documentStatus,
                            VendorType = OpenPayrollVendorConstant.Ocrolus
                        }
                    }
                }
            };
            var response = _openPayrollOcrolusDocStatusHandler.ProcessFlag(obj);

            Assert.Equal(resultStatus, response.FlagResult);
        }
    }
}