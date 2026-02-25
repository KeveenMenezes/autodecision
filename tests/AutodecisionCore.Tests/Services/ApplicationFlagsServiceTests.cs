using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services;
using AutodecisionCore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.Services
{
    public class ApplicationFlagsServiceTests
    {
        private readonly Mock<IApplicationCoreRepository> _mockApplicationCoreRepository;
        private readonly Mock<ILogger<ApplicationFlagsService>> _mockLogger;
        private readonly Mock<IFlagRepository> _mockFlagRepository;
        private readonly Mock<ITriggerService> _mockTriggerService;
        private readonly Mock<IApplicationFlagsBinder> _mockApplicationFlagsBinder;

        public ApplicationFlagsServiceTests()
        {
            _mockApplicationCoreRepository = new Mock<IApplicationCoreRepository>();
            _mockLogger = new Mock<ILogger<ApplicationFlagsService>>();
            _mockFlagRepository = new Mock<IFlagRepository>();
            _mockTriggerService = new Mock<ITriggerService>();
            _mockApplicationFlagsBinder = new Mock<IApplicationFlagsBinder>();
        }

        [Fact]
        public async Task AddApplicationFlagsToRegisterAddAllActiveFlags()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);
            var mockActiveFlags = ArrangeActiveFlags();

            _mockFlagRepository.Setup(r => r.GetAllActiveFlagsExceptWarningAsync()).ReturnsAsync(mockActiveFlags);
            var service = CreateApplicationFlagsService();

            // Act
            await service.AddApplicationFlagsToRegister(applicationCore);

            // Assert
            Assert.Equal(mockActiveFlags.Count, applicationCore.ApplicationFlags.Count);

            foreach (var flag in mockActiveFlags)
            {
                var applicationFlag = applicationCore.GetApplicationFlagByFlagCode(flag.Code);

                Assert.NotNull(applicationFlag);
                Assert.Equal((int)FlagResultEnum.InProcessing, applicationFlag.Status);
                Assert.Equal(flag.Code, applicationFlag.FlagCode);
            }
        }

        [Fact]
        public async Task FlagResponseStatusRegisterProcessesFlagResponseAndUpdateApplicationCore()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);
            applicationCore.AddFlag("236", false);

            var response = new ProcessFlagResponseEvent
            {
                LoanNumber = loanNumber,
                FlagCode = "236",
                Message = "Customer is required to link Open Banking or Open Payroll",
                ProcessedAt = DateTime.Now,
                FlagResult = FlagResultEnum.PendingApproval,
                InternalMessages = new List<InternalMessage>()
            };

            _mockApplicationCoreRepository.Setup(r => r.FindByLoanNumberIncludeApplicationFlagsAsync(loanNumber)).ReturnsAsync(applicationCore);
            _mockApplicationCoreRepository.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var service = CreateApplicationFlagsService();

            // Act
            await service.FlagResponseStatusRegister(response, applicationCore);

            // Assert
            var applicationFlag = applicationCore.GetApplicationFlagByFlagCode(response.FlagCode);

            Assert.NotNull(applicationFlag);
            Assert.Equal(response.Message, applicationFlag.Description);
            Assert.Equal(response.ProcessedAt, applicationFlag.ProcessedAt);
            Assert.Equal((int)response.FlagResult, applicationFlag.Status);

            _mockApplicationCoreRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        //[Fact]
        //public async Task AddApplicationFlagsToRegisterByIgnoringAsyncIgnoresSpecificFlag()
        //{
        //    // Arrange
        //    var loanNumber = "123456789";
        //    var applicationCore = new ApplicationCore(loanNumber);
        //    var ignoredFlagCode = FlagCode.CustomerIdentityFlag;
        //    var mockActiveFlags = ArrangeActiveFlags();

        //    _mockFlagRepository.Setup(r => r.GetAllActiveFlagsExceptWarningAsync()).ReturnsAsync(mockActiveFlags);

        //    var service = CreateApplicationFlagsService();

        //    // Act
        //    await service.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, ignoredFlagCode, "ApplicationFlagsServiceTests");

        //    // Assert
        //    var ignoredFlag = applicationCore.ApplicationFlags.FirstOrDefault(flag => flag.FlagCode == ignoredFlagCode);

        //    Assert.NotNull(ignoredFlag);
        //    Assert.Equal((int)FlagResultEnum.Ignored, ignoredFlag.Status);
        //}

        //[Fact]
        //public async Task AddApplicationFlagsToRegisterByIgnoringAsyncIgnoresMultipleFlags()
        //{
        //    // Arrange
        //    var loanNumber = "123456789";
        //    var applicationCore = new ApplicationCore(loanNumber);
        //    var ignoredFlagCodes = new[] { FlagCode.OpenBankingOrPayrollNotConnected, FlagCode.OpenBankingNotConnected };
        //    var mockActiveFlags = ArrangeActiveFlags();

        //    _mockFlagRepository.Setup(r => r.GetAllActiveFlagsExceptWarningAsync()).ReturnsAsync(mockActiveFlags);

        //    var service = CreateApplicationFlagsService();

        //    // Act
        //    await service.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, ignoredFlagCodes, "ApplicationFlagsServiceTests");

        //    // Assert
        //    foreach (var ignoredFlagCode in ignoredFlagCodes)
        //    {
        //        var ignoredFlag = applicationCore.ApplicationFlags.FirstOrDefault(flag => flag.FlagCode == ignoredFlagCode);

        //        Assert.NotNull(ignoredFlag);
        //        Assert.Equal((int)FlagResultEnum.Ignored, ignoredFlag.Status);
        //    }
        //}

        [Fact]
        public async Task AddOnlyInternalApplicationFlagsToRegisterAddsOnlyInternalFlags()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);
            var mockActiveInternalFlags = ArrangeActiveInternalFlags();

            _mockFlagRepository.Setup(r => r.GetAllActiveInternalFlagsExceptWarningAsync()).ReturnsAsync(mockActiveInternalFlags);

            var service = CreateApplicationFlagsService();

            // Act
            await service.AddOnlyInternalApplicationFlagsToRegister(applicationCore);

            // Assert
            foreach (var flag in mockActiveInternalFlags)
            {
                var applicationFlag = applicationCore.GetApplicationFlagByFlagCode(flag.Code);

                Assert.NotNull(applicationFlag);
                Assert.True(applicationFlag.InternalFlag);
            }
        }

        [Fact]
        public async Task HandleApplicationFlagsWhenNoFlagsExistBindApplicationFlagsIsCalled()
        {
            // Arrange
            var applicationCore = new ApplicationCore("123456789");
            var application = new Application();
            var reason = "old-autodecision";

            _mockApplicationFlagsBinder.Setup(x => x.BindApplicationFlagsAsync(application, applicationCore)).Verifiable();

            var service = CreateApplicationFlagsService();

            // Act
            await service.HandleApplicationFlags(application, applicationCore, reason);

            // Assert
            _mockApplicationFlagsBinder.Verify(x => x.BindApplicationFlagsAsync(application, applicationCore), Times.Once);
        }

        [Fact]
        public async Task HandleApplicationFlagsWhenFlagsExistBindApplicationFlagsIsNotCalled()
        {
            // Arrange
            var applicationCore = new ApplicationCore("123456789");
            applicationCore.AddFlag(FlagCode.DebitCardxBankAccountAnalysis, false);

            var application = new Application();
            var reason = "Connect Debit Card";

            var service = CreateApplicationFlagsService();

            _mockApplicationFlagsBinder.Setup(x => x.BindApplicationFlagsAsync(It.IsAny<Application>(), It.IsAny<ApplicationCore>()))
              .Verifiable();

            _mockTriggerService.Setup(x => x.GetTriggerFlagCodesByProcessingReasonAsync("Connect Debit Card"))
                     .ReturnsAsync(new List<string> { FlagCode.DebitCardxBankAccountAnalysis });

            // Act
            await service.HandleApplicationFlags(application, applicationCore, reason);

            // Assert
            _mockApplicationFlagsBinder.Verify(x => x.BindApplicationFlagsAsync(It.IsAny<Application>(), It.IsAny<ApplicationCore>()), Times.Never);
        }

        [Fact]
        public async Task BindApplicationFlagsByTriggerAsyncWhenNoFlagsExistReprocessAllFlagsIsNotCalled()
        {
            // Arrange
            var applicationCore = new ApplicationCore("123456789");
            var reason = "old-autodecision";

            var service = CreateApplicationFlagsService();

            // Act
            await service.BindApplicationFlagsByTriggerAsync(applicationCore, reason);

            // Assert
            _mockTriggerService.Verify(
                x => x.GetTriggerFlagCodesByProcessingReasonAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BindApplicationFlagsByTriggerAsyncWhenFlagsExistReprocessAllFlagsIsCalledWithCorrectFlags()
        {
            // Arrange
            var applicationCore = new ApplicationCore("123456789");
            applicationCore.AddFlag(FlagCode.EmploymentLength, false);
            applicationCore.AddFlag(FlagCode.OpenBankingOrPayrollNotConnected, false);
            applicationCore.AddFlag(FlagCode.OpenPayrollSSNDoesNotMatch, false);
            applicationCore.AddFlag(FlagCode.OpenPayrollNotConnected, false);

            var reason = "Connect With Argyle";

            _mockTriggerService.Setup(x => x.GetTriggerFlagCodesByProcessingReasonAsync(reason))
                              .ReturnsAsync(new List<string>
                              {
                                  FlagCode.EmploymentLength,
                                  FlagCode.OpenBankingOrPayrollNotConnected,
                                  FlagCode.OpenPayrollSSNDoesNotMatch,
                                  FlagCode.OpenPayrollNotConnected
                              });

            var service = CreateApplicationFlagsService();

            // Act
            await service.BindApplicationFlagsByTriggerAsync(applicationCore, reason);

            // Assert
            _mockTriggerService.Verify(
                x => x.GetTriggerFlagCodesByProcessingReasonAsync(reason), Times.Once);

            Assert.Equal((int)FlagResultEnum.InProcessing, applicationCore.GetApplicationFlagByFlagCode(FlagCode.EmploymentLength)?.Status);
            Assert.Equal((int)FlagResultEnum.InProcessing, applicationCore.GetApplicationFlagByFlagCode(FlagCode.OpenBankingOrPayrollNotConnected)?.Status);
            Assert.Equal((int)FlagResultEnum.InProcessing, applicationCore.GetApplicationFlagByFlagCode(FlagCode.OpenPayrollSSNDoesNotMatch)?.Status);
            Assert.Equal((int)FlagResultEnum.InProcessing, applicationCore.GetApplicationFlagByFlagCode(FlagCode.OpenPayrollNotConnected)?.Status);
        }

        private ApplicationFlagsService CreateApplicationFlagsService()
        {
            return new ApplicationFlagsService(
                _mockApplicationCoreRepository.Object,
                _mockLogger.Object,
                _mockFlagRepository.Object,
                _mockTriggerService.Object,
                _mockApplicationFlagsBinder.Object);
        }

        private static List<Flag> ArrangeActiveFlags()
        {
            return new List<Flag>()
            {
                new Flag(
                    code: FlagCode.OpenBankingOrPayrollNotConnected,
                    name: "Open Banking or Open Payroll Required",
                    description: "Customer is required to link Open Banking or Open Payroll"),
                new Flag(
                    code: FlagCode.OpenBankingNotConnected,
                    name: "Open Banking not connected",
                    description: "Open Banking not connected"),
                new Flag(
                    code: FlagCode.OpenPayrollNotConnected,
                    name: "Open Payroll not connected",
                    description: "Open Payroll not connected"),
                new Flag(
                    code: FlagCode.CustomerIdentityFlag,
                    name: "Customer Identity Flag",
                    description: "Customer Identity doesn't match"),
                new Flag(
                    code: FlagCode.EmploymentLength,
                    name: "Employment Length",
                    description: "Length of employment is less than 1 year")
            };
        }

        private static List<Flag> ArrangeActiveInternalFlags()
        {
            return new List<Flag>()
            {
                new Flag(
                    code: FlagCode.LoanVerification,
                    name: "Loan Verification",
                    description: "Loan Verification",
                    internalFlag: true,
                    isWarning: false
                    ),
                new Flag(
                    code: FlagCode.Flag209,
                    name: "Flag 209",
                    description: "Auto Approval Refi",
                    internalFlag: true,
                    isWarning: false
                    ),
                new Flag(
                    code: FlagCode.OpenForChanges,
                    name: "Open for changes",
                    description: "Open for changes",
                    internalFlag: true,
                    isWarning: true
                    )
            };
        }
    }
}