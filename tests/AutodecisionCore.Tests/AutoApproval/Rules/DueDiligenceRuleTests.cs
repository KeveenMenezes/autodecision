using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.ViewModels.Application;
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
    public class DueDiligenceRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private DueDiligenceRule _dueDiligenceRule;

        public DueDiligenceRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _dueDiligenceRule = new DueDiligenceRule(_mockFeatureToggle.Object, _mockLoger.Object);
        }

        [Fact]
        public void WhenDueDiligenceIsRejected_ThenRuleShouldReturnPending()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.LoanNumber = loanNumber;
            request.Employer = new Employer()
            {
                DueDiligenceStatus = (int)Contracts.Enums.DueDiligenceStatusEnum.Rejected
            };

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Pending;

            // Act
            _dueDiligenceRule.RunRule(request, response, applicationCore);
            var dueDiligenceRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DueDiligenceRule");

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.False(string.IsNullOrEmpty(dueDiligenceRuleResponse.Description));
            Assert.Equal(DueDiligenceRuleConstants.DueDiligenceRejected, dueDiligenceRuleResponse.Description);

        }

        [Fact]
        public void WhenDueDiligenceIsUnderReview_ThenRuleShouldReturnPending()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.LoanNumber = loanNumber;
            request.Employer = new Employer()
            {
                DueDiligenceStatus = (int)Contracts.Enums.DueDiligenceStatusEnum.UnderReview
            };

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Pending;

            // Act
            _dueDiligenceRule.RunRule(request, response, applicationCore);
            var dueDiligenceRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DueDiligenceRule");

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.False(string.IsNullOrEmpty(dueDiligenceRuleResponse.Description));
            Assert.Equal(DueDiligenceRuleConstants.DueDiligenceRejected, dueDiligenceRuleResponse.Description);

        }

        [Fact]
        public void WhenDueDiligenceIsApproved_ThenRuleShouldReturnApproved()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.LoanNumber = loanNumber;
            request.Employer = new Employer()
            {
                DueDiligenceStatus = (int)Contracts.Enums.DueDiligenceStatusEnum.Approved
            };

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Approved;

            // Act
            _dueDiligenceRule.RunRule(request, response, applicationCore);
            var dueDiligenceRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DueDiligenceRule");

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.True(string.IsNullOrEmpty(dueDiligenceRuleResponse.Description));

        }

        [Fact]
        public void WhenDueDiligenceIsMissing_ThenRuleShouldReturnError()
        {
            // Arrange
            var loanNumber = "123456789";
            var applicationCore = new ApplicationCore(loanNumber);

            var request = AutoApprovalTestUtils.CreateSetup();
            request.LoanNumber = loanNumber;

            var response = new AutoApprovalResponse();
            var expected = AutoApprovalResultEnum.Error;

            // Act
            _dueDiligenceRule.RunRule(request, response, applicationCore);
            var dueDiligenceRuleResponse = response.AutoApprovalRules.Find(x => x.RuleName == "DueDiligenceRule");

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.False(string.IsNullOrEmpty(dueDiligenceRuleResponse.Description));
        }
    }
}