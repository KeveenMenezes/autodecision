using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class TransunionScoresHandlerTests
    {
        private readonly Mock<ILogger<TransunionScoresHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlaghelper;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggleClient;

        public TransunionScoresHandlerTests()
        {
            _mockLogger = new Mock<ILogger<TransunionScoresHandler>>();
            _mockFlaghelper = new Mock<IFlagHelper>();
            _mockFeatureToggleClient = new Mock<IFeatureToggleClient>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlaghelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggleClient, false);
        }

        [Fact]
        public void GivenTransunionResultEqualsNullWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test"
                },
                TransunionResult = null
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenApplicationIsRefiAndOfacHitIsNotOneWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.Refi
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenOfacHitIsOneWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "1"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenScoreIsBelow70WhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    Score = 0
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenDobScoreIsBelow70WhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    DobScore = 0
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenAddressScoreIsBelow70WhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    AddressScore = 0
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenNameScoreIsBelow70WhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    NameScore = 0
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenSSNScoreIsBelow70WhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    SSNScore = 0
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenClarityIsNotNullAndMatchesWhenProcessingFlagThenFlagResultEqualsApproved()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    Score = 0,
                    DobScore = 0,
                    AddressScore = 0,
                    NameScore = 0,
                    SSNScore = 0
                },
                Clarity = new Clarity()
                {
                    DobMatchResult = "Match",
                    DobMatchDescription = "Full DOB available and matched input (within +/- 1 year if month.day exact)",
                    NameAddressMatchConfidence = 5,
                    NameAddressMatchDescription = "Exact match on first and last name; Exact match on address",
                    SSNNameAddressMatchConfidence = 5,
                    SSNNameAddressMatchDescription = "Exact SSN match, Exact Name match, Exact Address match"
                }
            };
            var expected = FlagResultEnum.Approved;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenAllDataIsCorrectWhenProcessingFlagThenFlagReturnsProcessed()
        {
            // Arrange
            var transunionScoresHandler = new TransunionScoresHandler(_mockLogger.Object, _mockFlaghelper.Object, _mockFeatureToggleClient.Object);
            _mockFeatureToggleClient.Setup(x => x.IsEnabled("new_autodecision_ignore_some_flags")).Returns(false);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "test",
                    Type = ApplicationType.NewLoan
                },
                TransunionResult = new TransunionResult()
                {
                    OfacHit = "0",
                    Score = 99,
                    DobScore = 99,
                    AddressScore = 99,
                    NameScore = 99,
                    SSNScore = 99
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = transunionScoresHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

    }
}