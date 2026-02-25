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
    public class DebitCardIssuerRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private DebitCardIssuerRule _debitCardIssueRule;

        public DebitCardIssuerRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _debitCardIssueRule = new DebitCardIssuerRule(_mockFeatureToggle.Object, _mockLoger.Object);
        }

        [Fact]
        public void RunRuleWhenFeatureToggleDisabledShouldReturnsCompletedTask()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(true);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);
            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.CityOfSweetwater, ApplicationProgram.LoansForAll);

            var response = new AutoApprovalResponse();

            // Act
            var result = _debitCardIssueRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
            Assert.Null(response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule"));
        }

        [Fact]
        public void RunRuleWhenExceptionThrownShouldReturnsErrorStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            //This test is missing some arrange information to throw an expected error

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");
            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Error, debitCardIssuerRuleResponse.Status);
        }

        [Fact]
        public void RunRuleWhenPaymentTypeOrFundingMethodAreNotDebitCardShouldReturnsIgnoredStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var description = "Payment type or funding method must be debit card.";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.ACH);

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Ignored, debitCardIssuerRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
            Assert.Equal(description, debitCardIssuerRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenApplicationHasDebitCardButDebitCardIsNotConnectedShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var description = "Debit Card not connected / The issuer of Debit Card must be the same of the customer's bank account.";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            request.DebitCard = new DebitCard(); //creating an empty object to represent no one connection

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, debitCardIssuerRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
            Assert.Equal(description, debitCardIssuerRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenApplicationHasDebitCardConnectAndIsNotInWebBankRolloutWithPaynearmeVendorShouldReturnsApprovedStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";

            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.PayNearMe);

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Approved, debitCardIssuerRuleResponse.Status);
            Assert.True(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
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
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.Tabapay);

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, debitCardIssuerRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
            Assert.Equal(description, debitCardIssuerRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenApplicationIsWebBankRolloutAndDebitCardVendorDoesntMatchShouldReturnsPendingStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);
            var description = "Customer's active debit card connection does not match rollout definition.";

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            request.Application.IsWebBankRollout = true;
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.PayNearMe);

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Pending, debitCardIssuerRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
            Assert.Equal(description, debitCardIssuerRuleResponse.Description);
        }

        [Fact]
        public void RunRuleWhenApplicationIsWebBankRolloutAndDebitCardVendorMatchesShouldReturnsApprovedStatus()
        {
            // Arrange
            _mockFeatureToggle.Setup(c => c.IsDisabled("AutoApprovalRuleDebitCardIssuer")).Returns(false);

            var request = AutoApprovalTestUtils.CreateSetup();
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            request.LoanNumber = loanNumber;
            request.Application = AutoApprovalTestUtils.CreateApplication(EmployersIds.UnitedStatesPostalService, ApplicationProgram.LoansForFeds, ApplicationFundingMethod.DebitCard);
            request.Application.IsWebBankRollout = true;
            request.DebitCard = AutoApprovalTestUtils.CreateDebitCardConnection(DebitCardVendor.Tabapay);

            var response = new AutoApprovalResponse();

            // Act
            _debitCardIssueRule.RunRule(request, response, applicationCore);
            var debitCardIssuerRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DebitCardIssuerRule");

            // Assert
            Assert.NotNull(debitCardIssuerRuleResponse);
            Assert.Equal(AutoApprovalResultEnum.Approved, debitCardIssuerRuleResponse.Status);
            Assert.True(string.IsNullOrEmpty(debitCardIssuerRuleResponse.Description));
        }
    }
}