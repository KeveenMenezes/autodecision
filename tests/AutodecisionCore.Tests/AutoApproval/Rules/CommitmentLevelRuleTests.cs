using AutodecisionCore.AutoApprovalCore.DTO;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Core.AutoApproval.Rules;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Extensions;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public class CommitmentLevelRuleTests
    {
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;

        private readonly Mock<ILogger<AutoApprovalManager>> _mockLoger;
        public CommitmentLevelRuleTests()
        {
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();
            _mockLoger = new Mock<ILogger<AutoApprovalManager>>();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [Fact]
        public void GivenANullNetIncomeThenResultEqualsPending()
        {
            // Arrange
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();
            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 115m
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void GivenACommitmentLevelGreaterThanMaxValueThenResultEqualsPending()
        {
            // Arrange 
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 155.35m,
                VerifiedNetIncome = 1497.73m
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;
            var commitmentLevelCustomer = mockAutoApprovalRequest.Application.AmountOfPayment / mockAutoApprovalRequest.Application.VerifiedNetIncome.Value;
            
            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);
            var commitmentLevelEmployer = mockAutoApprovalRequest.CreditPolicy.EmployerRules.EmployerRulesItems[0].Max;
            var maxValue = Decimal.Parse(commitmentLevelEmployer);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.Equal($"The customer's commitment level {(commitmentLevelCustomer * 100).ToString("N2")}% exceeds the maximum value allowed {(maxValue * 100).ToString("N0")}%", response.AutoApprovalRules.Last().Description);

        }

        [Fact]
        public void GivenACommitmentLevelLessThanMaxValueThenResultEqualsApproved()
        {
            // Arrange
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);
            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 90m,
                VerifiedNetIncome = 1000m
            };
          
            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Approved;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void TryGivenACommitmentLevelWithoutAmountOfPaymentAndVerifiedNetIncome()
        {
            // Arrange
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);
            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 0,
                VerifiedNetIncome = 0
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }

        [Fact]
        public void TryGivenACommitmentLevelWithoutAVerifiedNetIncome()
        {
            // Arrange
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);
            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 155m,
                VerifiedNetIncome = 0
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }
  
        [Fact]
        public void GivenAdditionalIncomeNullThanResultInNormalCommitmentLevelCalculate()
        {
            // Arrange 
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 40m,
                VerifiedNetIncome = 227m,
                AdditionalIncome = null
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Pending;
            var commitmentLevelCustomer = mockAutoApprovalRequest.Application.AmountOfPayment / mockAutoApprovalRequest.Application.VerifiedNetIncome.Value;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);
            var commitmentLevelEmployer = mockAutoApprovalRequest.CreditPolicy.EmployerRules.EmployerRulesItems[0].Max;
            var maxValue = Decimal.Parse(commitmentLevelEmployer);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
            Assert.Equal($"The customer's commitment level {(commitmentLevelCustomer * 100).ToString("N2")}% exceeds the maximum value allowed {(maxValue * 100).ToString("N0")}%", response.AutoApprovalRules.Last().Description);

        }

        [Fact]
        public void GivenAdditionalIncomeThanResultInNormalCommitmentLevelCalculate()
        {
            // Arrange 
            var commitmentLevelRule = new CommitmentLevelRule(_mockFeatureToggle.Object, _mockLoger.Object);

            var mockAutoApprovalRequest = AutoApprovalTestUtils.CreateSetup();

            mockAutoApprovalRequest.LoanNumber = "123456";
            mockAutoApprovalRequest.Application = new Contracts.ViewModels.Application.Application()
            {
                Type = ApplicationType.Refi,
                Program = ApplicationProgram.LoansForAll,
                FundingMethod = "debit_card",
                PaymentType = "ach",
                UwCluster = "A",
                AmountOfPayment = 40m,
                VerifiedNetIncome = 227m,
                AdditionalIncome = 300
            };

            var response = new AutoApprovalResponse();

            var expected = AutoApprovalResultEnum.Approved;
            var commitmentLevelCustomer = mockAutoApprovalRequest.Application.AmountOfPayment / mockAutoApprovalRequest.Application.VerifiedNetIncome.Value;

            // Act
            commitmentLevelRule.RunRule(mockAutoApprovalRequest, response, null);
            var commitmentLevelEmployer = mockAutoApprovalRequest.CreditPolicy.EmployerRules.EmployerRulesItems[0].Max;
            var maxValue = Decimal.Parse(commitmentLevelEmployer);

            // Assert
            Assert.Equal(expected, response.AutoApprovalRules.Last().Status);
        }
    }
}
