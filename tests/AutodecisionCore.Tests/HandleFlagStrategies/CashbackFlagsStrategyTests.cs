using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using Moq;

namespace AutodecisionCore.Tests.HandleFlagStrategies
{
    public class CashbackFlagsStrategyTests
    {
        private readonly Mock<IApplicationFlagsServiceFactory> _mockApplicationFlagsServiceFactory;
        private readonly Mock<IApplicationFlagsService> _mockApplicationFlagsService;
        private CashbackFlagsStrategy _cashbackFlagsStrategy;

        public CashbackFlagsStrategyTests()
        {
            _mockApplicationFlagsServiceFactory = new Mock<IApplicationFlagsServiceFactory>();
            _mockApplicationFlagsService = new Mock<IApplicationFlagsService>();
            _cashbackFlagsStrategy = new CashbackFlagsStrategy(_mockApplicationFlagsServiceFactory.Object);
        }

        //[Fact]
        //public async Task WhenApplicationIsCashbackProductTheCashbackFlagsStrategyAddsOnlyInternalFlagsIgnoringEmploymentLengthFlag()
        //{
        //    // Arrange
        //    string loanNumber = "123456789";
        //    _mockApplicationFlagsServiceFactory.Setup(x => x.GetService()).Returns(_mockApplicationFlagsService.Object);

        //    var application = new Application { ProductId = ApplicationProductId.Cashback };
        //    var applicationCore = new ApplicationCore(loanNumber);

        //    // Act
        //    await _cashbackFlagsStrategy.BindFlagsAsync(application, applicationCore);

        //    // Assert
        //    _mockApplicationFlagsService.Verify(
        //        x => x.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.EmploymentLength, "CashbackFlagsStrategyTests"),
        //        Times.Once);
        //}

        //[Fact]
        //public async Task WhenApplicationIsLowIncomeCashbackProductTheCashbackFlagsStrategyAddsOnlyInternalFlagsIgnoringEmploymentLengthFlag()
        //{
        //    // Arrange
        //    string loanNumber = "123456789";
        //    _mockApplicationFlagsServiceFactory.Setup(x => x.GetService()).Returns(_mockApplicationFlagsService.Object);

        //    var application = new Application { ProductId = ApplicationProductId.LowIncomeCashback };
        //    var applicationCore = new ApplicationCore(loanNumber);

        //    // Act
        //    await _cashbackFlagsStrategy.BindFlagsAsync(application, applicationCore);

        //    // Assert
        //    _mockApplicationFlagsService.Verify(
        //        x => x.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.EmploymentLength, "CashbackFlagsStrategyTests"),
        //        Times.Once);
        //}
    }
}