using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class FactorTrustInconsistencyHandlerTest
    {
        private readonly Mock<ILogger<FactorTrustInconsistencyHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public FactorTrustInconsistencyHandlerTest()
        {
            _mockLogger = new Mock<ILogger<FactorTrustInconsistencyHandler>>(); 
            _mockFlagHelper = new Mock<IFlagHelper>(); 

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void NoActiveRecordsFound()
        {
            // Arrange
            var factorTrustHandler = new FactorTrustInconsistencyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = null
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = factorTrustHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Theory]
        [InlineData("active duty", true)]
        [InlineData("active duty", false)]
        [InlineData("not active duty", true)]
        [InlineData("not active duty", false)]
        public void MilitaryStatusAndHasOrNotFactorTrustInconsistency(string mla, bool hasFactorTrustInconsistency)
        {
            // Arrange
            var factorTrustHandler = new FactorTrustInconsistencyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = mla,
                    HasFactorTrustInconsistency = hasFactorTrustInconsistency
                }
            };
            var expected = hasFactorTrustInconsistency ? FlagResultEnum.PendingApproval : FlagResultEnum.Processed;

            // Act
            var actual = factorTrustHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("-1")]
        public void IndependentMilitaryStatusAndHasFactorTrustInconsistency(string mla)
        {
            // Arrange
            var factorTrustHandler = new FactorTrustInconsistencyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = mla,
                    HasFactorTrustInconsistency = false
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = factorTrustHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}
