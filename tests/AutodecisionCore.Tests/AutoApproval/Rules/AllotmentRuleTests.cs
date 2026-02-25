
using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class AllotmentRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private AllotmentRule _allotmentRule;

        public AllotmentRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _allotmentRule = new AllotmentRule(_mockFeatureToggle.Object, _mockLoger.Object);
        }

        [Fact]
        public void GivenAnApplicationWithDebitCardAsPaymentShouldBeIgnored()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);
            var application = AutoApprovalTestUtils.MockApplication(paymentType: "debit_card");
            var lastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication(),
                AutoApprovalTestUtils.MockLastApplication()
            };

            var request = AutoApprovalTestUtils.CreateSetup();
            request.LoanNumber = loanNumber;
            request.Application = application;
            request.LastApplications = lastApplications;

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Ignored;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void GivenASameAlltomentInformationShouldBeApproved()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.SetApplicationAndLastApplicationsWithSameAllotmentInformation();

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Approved;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }


        [Fact]
        public void GivenADifferentReconciliationSystem()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.SetApplicationAndLastApplicationsWithDifferentReconciliationSystems();

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.PendingAllotment;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void GivenALastApplicationWithDifferentAmountThenResultEqualsPending()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.SetApplicationAndLastApplicationsWithDifferentAmounts();

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.PendingAllotment;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void AllotmentRuleWhenFirstLoanWithoutOpenPayrollMandatoryShouldReturnsPendingAllotment()
        {
            // Arrange
            var loanNumber = "123456789";
            var application = AutoApprovalTestUtils.MockApplication(loanNumber: loanNumber);

            var applicationCore = new ApplicationCore(loanNumber);
            var request = AutoApprovalTestUtils.CreateSetup();

            request.LoanNumber = loanNumber;
            request.Application = application;
            request.LastApplications = new List<LastApplication>();

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.PendingAllotment;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);
            var allotmentRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "AllotmentRule");


            // Assert
            Assert.NotNull(allotmentRuleResponse);
            Assert.Equal(expected, allotmentRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(allotmentRuleResponse.Description));
            Assert.Equal(AllotmentRuleConstants.FirstLoan, allotmentRuleResponse.Description);
        }

        [Fact]
        public void AllotmentRuleWhenFirstLoanWithOpenPayrollMandatoryRuleButCustomerDoesntConnectedItShouldReturnsIgnoredStatus()
        {
            // Arrange
            var loanNumber = "123456789";
            var application = AutoApprovalTestUtils.MockApplication(loanNumber: loanNumber);

            var applicationCore = new ApplicationCore(loanNumber);
            var request = AutoApprovalTestUtils.CreateSetup();

            request.LoanNumber = loanNumber;
            request.Application = application;
            request.LastApplications = new List<LastApplication>();
            request.CreditPolicy.EmployerRules.EmployerRulesItems.Add(new EmployerRulesItem()
            {
                Key = "open_payroll_mandatory",
                Required = true,
            });

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Ignored;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);
            var allotmentRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "AllotmentRule");

            // Assert
            Assert.NotNull(allotmentRuleResponse);
            Assert.Equal(expected, allotmentRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(allotmentRuleResponse.Description));
            Assert.Equal(AllotmentRuleConstants.OpenPayrollMandatory, allotmentRuleResponse.Description);
        }

        [Fact]
        public void AllotmentRuleWhenFirstLoanWithOpenPayrollMandatoryRuleAndCustomerConnectedItShouldReturnsPendingAllotment()
        {
            // Arrange
            var loanNumber = "123456789";
            var application = AutoApprovalTestUtils.MockApplication(loanNumber: loanNumber);

            var applicationCore = new ApplicationCore(loanNumber);
            var request = AutoApprovalTestUtils.CreateSetup();

            request.LoanNumber = loanNumber;
            request.Application = application;
            request.LastApplications = new List<LastApplication>();

            request.CreditPolicy.EmployerRules.EmployerRulesItems.Add(new EmployerRulesItem()
            {
                Key = "open_payroll_mandatory",
                Required = true,
            });

            request.OpenPayroll = new OpenPayroll
            {
                Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                        }
                    }
            };

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.PendingAllotment;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);
            var allotmentRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "AllotmentRule");

            // Assert
            Assert.NotNull(allotmentRuleResponse);
            Assert.Equal(expected, allotmentRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(allotmentRuleResponse.Description));
            Assert.Equal(AllotmentRuleConstants.FirstLoan, allotmentRuleResponse.Description);
        }


        [Fact]
        public void AllotmentRuleWhenNewLoanIsFromAnotherProgramItShouldReturnsPendingAllotment()
        {
            // Arrange
            var loanNumber = "123456789";

            var application = AutoApprovalTestUtils.MockApplication(program: ApplicationProgram.LoansForAll);
            var lastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication(amount: 150, status:ApplicationStatus.Booked, loanNumber:"123456788", program:ApplicationProgram.LoansAtWork),
                AutoApprovalTestUtils.MockLastApplication(amount: 150, status:ApplicationStatus.Liquidated, loanNumber:"123456799", program:ApplicationProgram.LoansAtWork),
            };

            var applicationCore = new ApplicationCore(loanNumber);
            var request = AutoApprovalTestUtils.CreateSetup();

            request.LoanNumber = loanNumber;
            request.Application = application;
            request.LastApplications = lastApplications;

            request.CreditPolicy.EmployerRules.EmployerRulesItems.Add(new EmployerRulesItem()
            {
                Key = "open_payroll_mandatory",
                Required = true,
            });

            request.OpenPayroll = new OpenPayroll
            {
                Connections = new List<OpenPayrollConnection> {
                        new OpenPayrollConnection()
                        {
                            ConnectedAt = DateTime.Now,
                            IsActive = true,
                        }
                    }
            };

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.PendingAllotment;

            // Act
            _allotmentRule.RunRule(request, response, applicationCore);
            var allotmentRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "AllotmentRule");

            // Assert
            Assert.NotNull(allotmentRuleResponse);
            Assert.Equal(expected, allotmentRuleResponse.Status);
            Assert.False(string.IsNullOrEmpty(allotmentRuleResponse.Description));
            Assert.Equal(AllotmentRuleConstants.ProgramChanged, allotmentRuleResponse.Description);
        }
    }
}