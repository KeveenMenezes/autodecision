using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.AutoApprovalCore.Rules;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class ApplicationParametersRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;
        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        private readonly Mock<ICreditRiskService> _mockCreditRiskService;

        public ApplicationParametersRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            _mockCreditRiskService = new Mock<ICreditRiskService>();
        }

        [Fact]
        public void GivenAFundingMethodThatIsNotAllowedThenResultEqualsPending()
        {
            // Arrange
            var ApplicationParametersRule = new ApplicationParametersRule(_mockFeatureToggle.Object, _mockLoger.Object, _mockCreditRiskService.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A"
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            ApplicationParametersRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void GivenAPaymentTypeThatIsAllowedThenResultEqualsApproved()
        {
            // Arrange
            var ApplicationParametersRule = new ApplicationParametersRule(_mockFeatureToggle.Object, _mockLoger.Object, _mockCreditRiskService.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "allotment",
                UwCluster = "A"
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Approved;

            // Act
            ApplicationParametersRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void GivenAUwClusterThatIsNotAllowedThenResultEqualsPending()
        {
            // Arrange
            var ApplicationParametersRule = new ApplicationParametersRule(_mockFeatureToggle.Object, _mockLoger.Object, _mockCreditRiskService.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "ach",
                PaymentType = "allotment",
                UwCluster = "A"
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            ApplicationParametersRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }






    }
}
