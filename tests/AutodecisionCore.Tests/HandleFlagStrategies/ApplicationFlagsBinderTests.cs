using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.HandleFlagStrategies;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using Moq;

namespace AutodecisionCore.Tests.HandleFlagStrategies
{
    public class ApplicationFlagsBinderTests
    {
        private readonly Mock<IDefaultFlagsStrategy> _mockDefaultStrategy;
        private readonly Mock<IApplicationFlagsStrategy> _mockStrategy;
        private readonly List<IApplicationFlagsStrategy> _mockStrategies;

        public ApplicationFlagsBinderTests()
        {
            _mockDefaultStrategy = new Mock<IDefaultFlagsStrategy>();
            _mockStrategy = new Mock<IApplicationFlagsStrategy>();
            _mockStrategies = new List<IApplicationFlagsStrategy>();
        }

        [Fact]
        public async Task BindApplicationFlagsAsyncWithoutCompatibleStrategiesUseDefaultStrategy()
        {
            // Arrange
            var loanNumber = "123456789";
            var application = new Application(); 
            var applicationCore = new ApplicationCore(loanNumber); 
            
            var binder = new ApplicationFlagsBinder(_mockStrategies, _mockDefaultStrategy.Object);

            // Act
            await binder.BindApplicationFlagsAsync(application, applicationCore);

            // Assert
            _mockDefaultStrategy.Verify(
                x => x.BindFlagsAsync(It.IsAny<Application>(), It.IsAny<ApplicationCore>()),
                Times.Once);
        }

        [Fact]
        public async Task BindApplicationFlagsAsyncUseCompatibleStrategy()
        {
            // Arrange
            var loanNumber = "123456789";
            
            _mockStrategy.Setup(x => x.CanHandle(It.IsAny<Application>(), It.IsAny<ApplicationCore>())).Returns(true);
            _mockStrategies.Add(_mockStrategy.Object);

            var application = new Application();
            var applicationCore = new ApplicationCore(loanNumber);
            
            var binder = new ApplicationFlagsBinder(_mockStrategies, _mockDefaultStrategy.Object);

            // Act
            await binder.BindApplicationFlagsAsync(application, applicationCore);

            // Assert
            _mockStrategy.Verify(
                x => x.BindFlagsAsync(It.IsAny<Application>(), It.IsAny<ApplicationCore>()),
                Times.Once);
        }

        //[Fact]
        //public async Task BindApplicationFlagsAsyncUseHighPriorityStrategy()
        //{
        //    // Arrange
        //    var loanNumber = "123456789";

        //    var firstMockStrategy = new Mock<IApplicationFlagsStrategy>();
        //    firstMockStrategy.Setup(x => x.CanHandle(It.IsAny<Application>(), It.IsAny<ApplicationCore>())).Returns(true);
        //    firstMockStrategy.Setup(x => x.Priority).Returns(2);

        //    var secondMockStrategy = new Mock<IApplicationFlagsStrategy>();
        //    secondMockStrategy.Setup(x => x.CanHandle(It.IsAny<Application>(), It.IsAny<ApplicationCore>())).Returns(true);
        //    secondMockStrategy.Setup(x => x.Priority).Returns(3);

        //    _mockStrategies.Add(firstMockStrategy.Object);
        //    _mockStrategies.Add(secondMockStrategy.Object);

        //    var application = new Application();
        //    var applicationCore = new ApplicationCore(loanNumber);
            
        //    var binder = new ApplicationFlagsBinder(_mockStrategies, _mockDefaultStrategy.Object);

        //    // Act
        //    await binder.BindApplicationFlagsAsync(application, applicationCore);

        //    // Assert
        //    secondMockStrategy.Verify(
        //        x => x.BindFlagsAsync(It.IsAny<Application>(), It.IsAny<ApplicationCore>()),
        //        Times.Once);
        //}
    }
}