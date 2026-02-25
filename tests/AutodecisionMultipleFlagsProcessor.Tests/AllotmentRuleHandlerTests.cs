using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class AllotmentRuleHandlerTests
    {
        private readonly Mock<ILogger<AllotmentRuleHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        public AllotmentRuleHandlerTests()
        {
            _mockLogger = new Mock<ILogger<AllotmentRuleHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenPaymentTypeIsntAllotmentAndSplitDirectDepositShouldIgnoreFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {
                    PaymentType = "debit_card"
                }
            };
            var handler = new AllotmentRuleHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Ignored, resposta.FlagResult);
        }

        [Fact]
        public void WhenLastApplicationsIsNullShouldRaiseFlag()
        {
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {
                    PaymentType = "allotment"
                },
                LastApplications = new List<LastApplication>()

            };
            var handler = new AllotmentRuleHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public void WhenAmountOfPaymentOrReconciliationSystemIsDifferentOfLastApplicationShouldRaiseFlag()
        {
            var lastApplications = new List<LastApplication>() {
                new LastApplication()
                {
                AmountOfPayment = 10.00M,
                ReconciliationSystem = "Test",
                Status = "6"
                }
            };
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {
                    PaymentType = "allotment",
                    AmountOfPayment = 100.00M,
                    ReconciliationSystem = "Test"
                },
                LastApplications = lastApplications

            };
            var handler = new AllotmentRuleHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);
        }
        [Fact]
        public void WhenallConditionsAreCorrectShouldProcessFlag()
        {
            var lastApplications = new List<LastApplication>() {
                new LastApplication()
                {
                AmountOfPayment = 100.00M,
                ReconciliationSystem = "Test",
                Status = "6"
                }
            };
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer()
                {
                    Id = 413667
                },
                Application = new Application()
                {
                    PaymentType = "allotment",
                    AmountOfPayment = 100.00M,
                    ReconciliationSystem = "Test"
                },
                LastApplications = lastApplications

            };
            var handler = new AllotmentRuleHandler(_mockLogger.Object, _mockFlagHelper.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);
        }
    }
}
