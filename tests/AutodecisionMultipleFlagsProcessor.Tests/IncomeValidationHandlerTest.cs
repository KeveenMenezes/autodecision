using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{

    public class IncomeValidationHandlerTest
    {
        private readonly Mock<ILogger<IncomeValidationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly IncomeValidationHandler _handler;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;

        public IncomeValidationHandlerTest()
        {
            _mockLogger = new Mock<ILogger<IncomeValidationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();
            _handler = new IncomeValidationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockFeatureToggleClient.Object);

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, true);
        }

        [Fact]
        public void FlagIgnoredByApplicationTypeNotAllowedTest()
        {
            // Arrange
            var expected = FlagResultEnum.Ignored;

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "1",
                    Status = "3",
                    Type = "2",
                    ProductId = 1,
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
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("FeatureLineAssignment")).Returns(false);
            // Act
            var actual = _handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Theory]
        [InlineData(FlagResultEnum.PendingApproval, StatusIncome.Pending, "There are pending income verifications.")]
        [InlineData(FlagResultEnum.Processed, StatusIncome.Approved, "Income proven by the customer")]
        [InlineData(FlagResultEnum.PendingApproval, StatusIncome.Reproved, "Income not approved due to discrepancies found during document analysis.")]
        public void When_Income_Exist_Test(FlagResultEnum statusFlag, StatusIncome statusIncome, string messageExpected)
        {
            // Arrange
            var income = new TotalIncome()
            {
                ApplicationId = 1,
                TotalAmount = 1500,
                StatusDescription = Enum.GetName(typeof(StatusIncome), statusIncome),
                PayFrequency = "Weekly",
                Status = statusIncome
            };

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "123456789",
                    Program = "2",
                    Status = "3",
                    Type = "1",
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
                TotalIncome = income
            };

            // Act
            var actual = _handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(statusFlag, actual.FlagResult);
            Assert.Equal(messageExpected, actual.Message);
        }
    }
}
