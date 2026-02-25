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
    public class CashlessFlagsStrategyTests
    {
        private readonly Mock<IApplicationFlagsServiceFactory> _mockApplicationFlagsServiceFactory;
        private readonly Mock<IApplicationFlagsService> _mockApplicationFlagsService;
        private CashlessFlagsStrategy _cashlessFlagsStrategy;

        public CashlessFlagsStrategyTests()
        {
            _mockApplicationFlagsServiceFactory = new Mock<IApplicationFlagsServiceFactory>();
            _mockApplicationFlagsService = new Mock<IApplicationFlagsService>();
            _cashlessFlagsStrategy = new CashlessFlagsStrategy(_mockApplicationFlagsServiceFactory.Object);
        }

        [Fact]
        public async Task CashlessFlagsStrategyAddsOnlyInternalFlagsForDebitCardAsPaymentType()
        {
            // Arrange
            var loanNumber = "123456789";
            _mockApplicationFlagsServiceFactory.Setup(x => x.GetService()).Returns(_mockApplicationFlagsService.Object);

            var application = new Application { ProductId = ApplicationProductId.Cashless, PaymentType = PayrollType.DebitCard };
            var applicationCore = new ApplicationCore(loanNumber);
            
            // Act
            await _cashlessFlagsStrategy.BindFlagsAsync(application, applicationCore);

            // Assert
            _mockApplicationFlagsService.Verify(
                x => x.AddOnlyInternalApplicationFlagsToRegister(applicationCore),
                Times.Once);
            _mockApplicationFlagsService.Verify(
                x => x.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.CustomerIdentityFlag, "CashlessFlagsStrategyTests"),
                Times.Never);
        }

        //[Fact]
        //public async Task CashlessFlagsStrategyAddsInternalFlagsForPaymentDifferentFromDebitCard()
        //{
        //    // Arrange
        //    var loanNumber = "123456789";
        //    _mockApplicationFlagsServiceFactory.Setup(x => x.GetService()).Returns(_mockApplicationFlagsService.Object);

        //    var application = new Application { ProductId = ApplicationProductId.Cashless, PaymentType = PayrollType.Allotment };
        //    var applicationCore = new ApplicationCore(loanNumber);
            
        //    // Act
        //    await _cashlessFlagsStrategy.BindFlagsAsync(application, applicationCore);

        //    // Assert
        //    _mockApplicationFlagsService.Verify(
        //        x => x.AddOnlyInternalApplicationFlagsToRegister(applicationCore),
        //        Times.Never);
        //    _mockApplicationFlagsService.Verify(
        //        x => x.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.CustomerIdentityFlag, "CashlessFlagsStrategyTests"),
        //        Times.Once);
        //}
    }
}