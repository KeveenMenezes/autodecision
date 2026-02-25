using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class LenghtOfEmploymentRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private LengthOfEmploymentRule _lengthOfEmploymentRule;

        public LenghtOfEmploymentRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _lengthOfEmploymentRule = new LengthOfEmploymentRule(_mockFeatureToggle.Object, _mockLoger.Object);
        }

        [Fact]
        public void WhenGivenAVerifiedDateLesserThanCreditPolicyShouldBePendingStatus()
        {
            // Arrange
            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Pending;

            request.LoanNumber = "123456";
            request.Application = request.Application = AutoApprovalTestUtils.MockApplication();
            request.Application.VerifiedDateOfHire = DateTime.Today.AddDays(-364);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication()
            };

            // Act
            _lengthOfEmploymentRule.RunRule(request, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void WhenGivenAVerifiedDateGreaterThanCreditPolicyShouldBeApprovedStatus()
        {
            // Arrange
            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Approved;

            request.LoanNumber = "123456";
            request.Application = AutoApprovalTestUtils.MockApplication();
            request.Application.VerifiedDateOfHire = DateTime.Today.AddDays(-366);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication()
            };

            // Act
            _lengthOfEmploymentRule.RunRule(request, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void WhenCustomerHasAPaidInFullApplicationShouldBeIgnoredStatus()
        {
            // Arrange
            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Ignored;

            request.LoanNumber = "123456";
            request.Application = AutoApprovalTestUtils.MockApplication();
            request.Application.VerifiedDateOfHire = DateTime.Today.AddDays(-366);
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication(amount: 150, status: ApplicationStatus.Liquidated)
            };

            // Act
            _lengthOfEmploymentRule.RunRule(request, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void WhenNotGivenAVerifiedDateThanCreditPolicyShouldBePendingStatus()
        {
            // Arrange
            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Pending;

            request.LoanNumber = "123456";
            request.Application = AutoApprovalTestUtils.MockApplication();
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication()
            };

            // Act
            _lengthOfEmploymentRule.RunRule(request, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }


        [Fact]
        public void WhenNotHaveAVerifiedDateHireThanShouldBePendingStatus()
        {
            // Arrange
            var request = AutoApprovalTestUtils.CreateSetup();
            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Pending;

            request.LoanNumber = "123456";
            request.Application = AutoApprovalTestUtils.MockApplication();
            request.LastApplications = new List<LastApplication>()
            {
                AutoApprovalTestUtils.MockLastApplication()
            };

            // Act
            _lengthOfEmploymentRule.RunRule(request, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }
    }
}