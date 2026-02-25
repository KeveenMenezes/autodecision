using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.Services
{
    public class CreditRiskServiceTests
    {
        private readonly Mock<ILogger<CreditRiskService>> _loggerMock;
        private readonly Mock<IFeatureToggleClient> _featureToggleMock;
        private readonly CreditRiskService _creditRiskService;

        public CreditRiskServiceTests()
        {
            _loggerMock = new Mock<ILogger<CreditRiskService>>();
            _featureToggleMock = new Mock<IFeatureToggleClient>();
            _creditRiskService = new CreditRiskService(_loggerMock.Object, _featureToggleMock.Object);

        }

        [Fact]
        public void ValidateAllowedLoanToCalculateScore_ValidInputs_ReturnsTrue()
        {
            // Arrange
            var application = new Application
            {
                LoanNumber = "LN123456",
                Program = BMGMoneyProgram.LoansForFeds, 
                Type = ApplicationType.NewLoan 
            };

            var employer = new Employer
            {
                SubProgramId = BMGMoneySubProgram.LoansForFeds 
            };

            _featureToggleMock.Setup(x => x.IsDisabled(It.IsAny<string>())).Returns(true);
            _featureToggleMock.Setup(x => x.IsEnabled(It.IsAny<string>())).Returns(false);

            // Act
            var result = _creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateAllowedLoanToCalculateScore_InvalidProgram_ReturnsFalse()
        {
            // Arrange
            var application = new Application
            { 
                LoanNumber = "LN123456",
                Program = "InvalidProgram", 
                Type = ApplicationType.NewLoan 
            };

            var employer = new Employer
            {
                SubProgramId = BMGMoneySubProgram.LoansForFeds 
            };

            _featureToggleMock.Setup(x => x.IsDisabled(It.IsAny<string>())).Returns(true);
            _featureToggleMock.Setup(x => x.IsEnabled(It.IsAny<string>())).Returns(false);

            // Act
            var result = _creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateAllowedLoanToCalculateScore_InvalidSubProgram_ReturnsFalse()
        {
            // Arrange
            var application = new Application
            {
                LoanNumber = "LN123456",
                Program = BMGMoneyProgram.LoansForFeds, 
                Type = ApplicationType.NewLoan 
            };

            var employer = new Employer
            {
                SubProgramId = BMGMoneySubProgram.Retired
            };

            _featureToggleMock.Setup(x => x.IsDisabled(It.IsAny<string>())).Returns(true);
            _featureToggleMock.Setup(x => x.IsEnabled(It.IsAny<string>())).Returns(false);

            // Act
            var result = _creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateAllowedLoanToCalculateScore_InvalidType_ReturnsFalse()
        {
            // Arrange
            var application = new Application
            {
                LoanNumber = "LN123456",
                Program = BMGMoneyProgram.LoansForFeds, 
                Type = ApplicationType.Refi
            };

            var employer = new Employer
            {
                SubProgramId = BMGMoneySubProgram.LoansForFeds 
            };

            _featureToggleMock.Setup(x => x.IsDisabled(It.IsAny<string>())).Returns(true);
            _featureToggleMock.Setup(x => x.IsEnabled(It.IsAny<string>())).Returns(false);

            // Act
            var result = _creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateAllowedLoanToCalculateScore_ValidType_ReturnsTrue()
        {
            // Arrange
            var application = new Application
            {
                LoanNumber = "LN123456",
                Program = BMGMoneyProgram.LoansForAll,
                Type = ApplicationType.NewLoan
            };

            var employer = new Employer
            {
                SubProgramId = BMGMoneySubProgram.LoansForAllMediumTurnOver
            };

            _featureToggleMock.Setup(x => x.IsDisabled(It.IsAny<string>())).Returns(true);
            _featureToggleMock.Setup(x => x.IsEnabled(It.IsAny<string>())).Returns(false);

            // Act
            var result = _creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer);

            // Assert
            Assert.True(result);
        }
    }
}
