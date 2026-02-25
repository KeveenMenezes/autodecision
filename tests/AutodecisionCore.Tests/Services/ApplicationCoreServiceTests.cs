using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services;
using Microsoft.Extensions.Logging;
using Moq;
using RedLockNet;

namespace AutodecisionCore.Tests.Services
{
    public class ApplicationCoreServiceTests
    {
        private readonly Mock<IApplicationCoreRepository> _applicationCoreRepositoryMock;
        private readonly Mock<IFlagRepository> _flagRepositoryMock;
        private readonly Mock<ILogger<ApplicationCoreService>> _loggerMock;
        private readonly Mock<IDistributedLockFactory> _redLockFactory;

        public ApplicationCoreServiceTests()
        {
            _applicationCoreRepositoryMock = new Mock<IApplicationCoreRepository>();
            _flagRepositoryMock = new Mock<IFlagRepository>();
            _loggerMock = new Mock<ILogger<ApplicationCoreService>>();
            _redLockFactory = new Mock<IDistributedLockFactory>();
        }

        [Fact]
        public async Task ApplicationCoreRegisterShouldCreateNewApplicationCoreAndRegisterFlagsWhenNotFound()
        {
            // Arrange
            string loanNumber = "123456789";
            string reason = "testReason";
            ApplicationCore applicationCore = null;

            _applicationCoreRepositoryMock.Setup(r => r.FindByLoanNumberAsync(loanNumber))
                .ReturnsAsync((ApplicationCore)null);

            _applicationCoreRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ApplicationCore>()))
                .Callback<ApplicationCore>(ac => applicationCore = ac)
                .Returns<ApplicationCore>(ac => Task.FromResult(ac));

            var service = new ApplicationCoreService(
                _applicationCoreRepositoryMock.Object,
                _flagRepositoryMock.Object,
                _loggerMock.Object,
                _redLockFactory.Object);

            // Act
            var result = await service.ApplicationCoreRegister(loanNumber);

            // Assert
            _applicationCoreRepositoryMock.Verify(x => x.CreateAsync(It.Is<ApplicationCore>(ac => ac.LoanNumber == loanNumber && ac.ProcessingVersion == 1)), Times.Once);
            //_applicationFlagsServiceMock.Verify(x => x.AddApplicationFlagsToRegister(It.IsAny<ApplicationCore>()), Times.Once);

            _loggerMock.Verify(x => x.Log(
                LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => string.Equals(
                    $"Registered Application Core for Loan Number: {loanNumber}",
                    v.ToString())), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            Assert.Equal(loanNumber, result.LoanNumber);
            Assert.Equal(1, result.ProcessingVersion);
        }

        [Fact]
        public async Task ApplicationCoreRegisterUpdatesExistingApplicationCoreAndReprocessesFlags()
        {
            // Arrange
            string loanNumber = "123456789";
            string reason = "Test Reason";
            var applicationCore = new ApplicationCore(loanNumber);
            applicationCore.AddFlag(flagCode: "244", internalFlag: false);

            _applicationCoreRepositoryMock.Setup(r => r.FindByLoanNumberAsync(loanNumber))
                .ReturnsAsync(applicationCore);
            _applicationCoreRepositoryMock.Setup(r => r.SaveChanges())
                .Returns(Task.CompletedTask);

            var service = new ApplicationCoreService(
                _applicationCoreRepositoryMock.Object,
                _flagRepositoryMock.Object,
                _loggerMock.Object,
                _redLockFactory.Object);

            // Act
            var result = await service.ApplicationCoreRegister(loanNumber);

            // Assert
            _applicationCoreRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<ApplicationCore>()), Times.Never);
            Assert.Equal(loanNumber, result.LoanNumber);
            Assert.Same(applicationCore, result);
        }
    }
}
