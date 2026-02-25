using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Handlers.Ocrolus;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenPayrollMismatchInformationHandlerTests
    {
        private readonly Mock<ILogger<OpenPayrollMismatchInformationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly OpenPayrollMismatchInformationHandler _openPayrollMismatchInformationHandler;

        public OpenPayrollMismatchInformationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OpenPayrollMismatchInformationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            _openPayrollMismatchInformationHandler = new OpenPayrollMismatchInformationHandler(_mockLogger.Object, _mockFlagHelper.Object);
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
            var response = _openPayrollMismatchInformationHandler.ProcessFlag(obj);

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
            var response = _openPayrollMismatchInformationHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
        }

        [Fact]
        public void ValidatePendingApprovalNoProfileInformation()
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
                        }
                    }
                }
            };
            var response = _openPayrollMismatchInformationHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
            Assert.Equal("There is no profile information on open payroll connection to validate data!", response.Message);
        }

        [Fact]
        public void ValidateProcessedIgnoreSSNValidation()
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
                    FirstName = "TEST",
                    LastName = "TEST",
                    Ssn = "123456789"
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = "6789",
                                Name = "TEST TEST",
                                EmployerName = "WALMART"
                            },
                            OldestPayDate = new DateTime(2021, 11, 20).Date,
                            IsActive = true,
                            ConnectedAt = DateTime.Now,
                        }
                    }
                }
            };
            var response = _openPayrollMismatchInformationHandler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.Processed, response.FlagResult);
        }

        [Theory]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", null, null, null, FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "TramOntiNa", "Minds Digital", "123456789", "*", "Minds Digital", "ViToR TRAMONTINA", FlagResultEnum.Processed)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", "*", "MindsDigital", "Vitor Tramontina", FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", "*", null, "Vitor Tramontina", FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", "*", "Minds Digital", null, FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", "7896", "Minds Digital", "Vitor Tramontina", FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", null, "Minds Digital", "Vitor Tramontina", FlagResultEnum.PendingApproval)]
        [InlineData("Vitor", "Tramontina", "Minds Digital", "123456789", "1234567777", "Minds Digital", "Vitor Tramontina", FlagResultEnum.PendingApproval)]
        public void ValidateMatchMethod(string customerFirstName, string customerLastName, string customerEmployerName, string customerSsn, string? openPayrollSsn, string? openPayrollEmployerName, string? openPayrollName, FlagResultEnum resultStatus)
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
                    EmployerName = customerEmployerName
                },
                Customer = new Customer()
                {
                    FirstName = customerFirstName,
                    LastName = customerLastName,
                    Ssn = customerSsn
                },
                OpenPayroll = new OpenPayroll
                {
                    Connections = new List<OpenPayrollConnection>{
                        new OpenPayrollConnection
                        {
                            ProfileInformation = new ProfileInformation
                            {
                                SSN = openPayrollSsn,
                                Name = openPayrollName,
                                EmployerName = openPayrollEmployerName
                            },
                            IsActive = true,
                            ConnectedAt = DateTime.Now,
                        }
                    }
                }
            };
            var response = _openPayrollMismatchInformationHandler.ProcessFlag(obj);

            Assert.Equal(resultStatus, response.FlagResult);
        }
    }
}