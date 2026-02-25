using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class CashlessRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private CashlessRule _cashlessRule;

        public CashlessRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _cashlessRule = new CashlessRule(_mockFeatureToggle.Object, _mockLoger.Object);
        }

        [Fact]
        public void RunRuleWhenFeatureToggleDisabledShouldReturnsCompletedTask()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(true);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);
            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.CityOfSweetwater, ApplicationProgram.LoansForAll);

            var response = new AutoApprovalResponse();

            // Act
            var result = _cashlessRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
            Assert.Null(response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule"));
        }

        [Fact]
        public void RunRuleWhenExceptionThrownShouldReturnsStatusWithError()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds);
            //This test is missing some arrange information to throw an expected error

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Error, cashlessRuleResponse.Status);
        }

        [Fact]
        public void RunRuleWhenEmployerIdIsBrowardCountyFLShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var loanNumber = "123456789";
            var description = "Employment verification required.";

            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var applicationCore = new ApplicationCore(loanNumber);

            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.BrowardCountyFL, ApplicationProgram.LoansForFeds);

            // Act
            var result = _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenLastApplicationProgramDiffersShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var description = "The last application program is different from the current application program.";

            var applicationCore = new ApplicationCore(loanNumber);
            
            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForAll)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenCustomerCreatedCashlessApplicationOutWebBankRolloutAndDebitCardWasConnectedShouldReturnsApprovedStatus()
        { 
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.PayNearMe);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Approved, cashlessRuleResponse.Status);
            Assert.Empty(cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenCustomerCreatedCashlessApplicationInWebBankRolloutAndDebitCardVendorDoesntMatchShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var description = "Customer's active debit card connection does not match rollout definition.";
            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.Application.IsWebBankRollout = true;
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.PayNearMe);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenCustomerCreatedCashlessApplicationInWebBankRolloutAndDebitCardVendorMatchesShouldReturnsApprovedStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.Application.IsWebBankRollout = true;
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.Tabapay);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Approved, cashlessRuleResponse.Status);
            Assert.Empty(cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenAgentCreatedCashlessApplicationInSameProgramWithoutDebitCardConnectionShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var description = "This application cannot be automatically approved.";

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds);
            request.DebitCard = new DebitCard(); //creating an empty object to represent no one connection
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenCustomerCreatedCashlessApplicationInSameProgramWithoutDebitCardConnectionShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var description = "Debit Card not connected / The issuer of Debit Card must be the same of the customer's bank account.";

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.DebitCard = new DebitCard(); //creating an empty object to represent no one connection
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenApplicationHasDebitCardConnectAndIsNotInWebBankRolloutWithPaynearmeVendorShouldReturnsApprovedStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleCashless")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.PayNearMe);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Approved, cashlessRuleResponse.Status);
            Assert.True(string.IsNullOrEmpty(cashlessRuleResponse.Description));
        }

        [Fact]
        public void RunRuleWhenApplicationHasDebitCardConnectAndIsNotInWebBankRolloutWithoutPaynearmeVendorShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var description = "Customer's active debit card connection doest not match Paynearme";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateCashlessApplicationWithDebitCardFunding(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, createdBy: "customer");
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.Tabapay);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.CreateLastApplicationAsNewLoanBooked(ApplicationProgram.LoansForFeds)
            };

            var response = new AutoApprovalResponse();

            // Act
            _cashlessRule.RunRule(request, response, applicationCore);
            var cashlessRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "CashlessRule");

            // Assert
            Assert.NotNull(cashlessRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, cashlessRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(cashlessRuleResponse.Description));
            Assert.Equal(description, cashlessRuleResponse.Description);
        }
    }
}