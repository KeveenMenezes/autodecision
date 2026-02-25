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
    public class RefiFlagsStrategyTests
    {
        private readonly Mock<IApplicationFlagsServiceFactory> _mockApplicationFlagsServiceFactory;
        private readonly Mock<IApplicationFlagsService> _mockApplicationFlagsService;
        private RefiFlagsStrategy _refiFlagsStrategy;

        public RefiFlagsStrategyTests()
        {
            _mockApplicationFlagsServiceFactory = new Mock<IApplicationFlagsServiceFactory>();
            _mockApplicationFlagsService = new Mock<IApplicationFlagsService>();
            _refiFlagsStrategy = new RefiFlagsStrategy(_mockApplicationFlagsServiceFactory.Object);
        }

        //[Fact]
        //public async Task WhenApplicationIsRefiTheRefiFlagsStrategyAddsOnlyInternalFlagsIgnoringEmploymentLengthFlag()
        //{
        //    // Arrange
        //    string loanNumber = "123456789";
        //    _mockApplicationFlagsServiceFactory.Setup(x => x.GetService()).Returns(_mockApplicationFlagsService.Object);

        //    var application = new Application { Type = ApplicationType.Refi };
        //    var applicationCore = new ApplicationCore(loanNumber);

        //    // Act
        //    await _refiFlagsStrategy.BindFlagsAsync(application, applicationCore);

        //    // Assert
        //    _mockApplicationFlagsService.Verify(
        //        x => x.AddApplicationFlagsToRegisterByIgnoringAsync(applicationCore, FlagCode.EmploymentLength, "RefiFlagsStrategyTests"),
        //        Times.Once);
        //}
    }
}