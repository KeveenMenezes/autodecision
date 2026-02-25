using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class ActiveMilitaryDutyHandlerTests
    {
        private readonly Mock<ILogger<ActiveMilitaryDutyHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public ActiveMilitaryDutyHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ActiveMilitaryDutyHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenFactorTrustIsNullWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
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
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenMlaIsActiveMilitaryDutyAndWhiteListNullWhenProcessingFlagThenFlagResultEqualsAutoDeny()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = "active duty"
                },
                WhiteList = null
            };
            var expected = FlagResultEnum.AutoDeny;

            // Act
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenMlaIsActiveMilitaryDutyAndWhiteListReasonIsNullOrEmptyWhenProcessingFlagThenFlagResultEqualsAutoDeny()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = "active duty"
                },
                WhiteList = new WhiteList()
                {
                    Reason = null
                }
            }; ;
            var expected = FlagResultEnum.AutoDeny;

            // Act
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenMlaIsActiveMilitaryDutyAndWhiteListReasonIsValidWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = "active duty"
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test"
                }
            }; ;
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenMlaIsDifferentFromNotActiveDutyWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = "test"
                },
                WhiteList = new WhiteList()
                {
                    Reason = "test"
                }
            }; ;
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenMlaIsNotActiveDutyWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var activeMilitaryDutyHandler = new ActiveMilitaryDutyHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                FactorTrust = new FactorTrust()
                {
                    Mla = "not active duty"
                },
                WhiteList = new WhiteList()
                {
                    Reason = null
                }
            }; ;
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = activeMilitaryDutyHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }
    }
}