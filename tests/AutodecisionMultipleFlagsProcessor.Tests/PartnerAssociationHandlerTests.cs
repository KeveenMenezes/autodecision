using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class PartnerAssociationHandlerTests
    {
        private readonly Mock<ILogger<PartnerAssociationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;

        public PartnerAssociationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<PartnerAssociationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Theory]
        [InlineData(ApplicationProgram.NotSet)]
        [InlineData(ApplicationProgram.LoansAtWork)]
        [InlineData(ApplicationProgram.LoansForFeds)]
        public void GivenApplicationProgramIsNotLfaWhenProcessingFlagThenFlagResultEqualsIgnored(string program)
        {
            // Arrange
            var partnerAssociationHandler = new PartnerAssociationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Program = program
                },
            };

            // Act
            var actual = partnerAssociationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(FlagResultEnum.Ignored, actual.FlagResult);
            Assert.NotNull(actual.Message);
            Assert.NotEmpty(actual.Message);
            Assert.Contains($"{ApplicationProgram.LoansForAll}", actual.Message);
        }

        [Fact]
        public void GivenEmployerIsNotAssociationWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var partnerAssociationHandler = new PartnerAssociationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Program = ApplicationProgram.LoansForAll,
                    EmployerId = 81,
                    EmployerIsAssociation = false
                },
            };

            // Act
            var actual = partnerAssociationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(FlagResultEnum.Ignored, actual.FlagResult);
            Assert.NotNull(actual.Message);
            Assert.NotEmpty(actual.Message);
            Assert.Contains($"{mockAutodecisionCompositeData.Application.EmployerId}", actual.Message);
        }

        [Fact]
        public void GivenApplicationPartnerIdIsNotEqualEmployerPartnerIdWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var partnerAssociationHandler = new PartnerAssociationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Program = ApplicationProgram.LoansForAll,
                    EmployerId = 81,
                    EmployerIsAssociation = true,
                    PartnerId = 1,
                    EmployerPartnerId = 2
                },
            };

            // Act
            var actual = partnerAssociationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(FlagResultEnum.Ignored, actual.FlagResult);
            Assert.NotNull(actual.Message);
            Assert.NotEmpty(actual.Message);
            Assert.Contains($"{mockAutodecisionCompositeData.Application.PartnerId}", actual.Message);
            Assert.Contains($"{mockAutodecisionCompositeData.Application.EmployerPartnerId}", actual.Message);
        }

        [Fact]
        public void GivenApplicationLfaHavePartnerIdAndEmployerIsAssociationWhenProcessingFlagThenFlagResultEqualsPendingApproval()
        {
            // Arrange
            var partnerAssociationHandler = new PartnerAssociationHandler(_mockLogger.Object, _mockFlagHelper.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Program = ApplicationProgram.LoansForAll,
                    EmployerId = 81,
                    EmployerIsAssociation = true,
                    PartnerId = 1,
                    EmployerPartnerId = 1,
                    PartnerName = "Partner Test"
                },
            };

            // Act
            var actual = partnerAssociationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(FlagResultEnum.PendingApproval, actual.FlagResult);
            Assert.NotNull(actual.Message);
            Assert.NotEmpty(actual.Message);
            Assert.Contains($"{mockAutodecisionCompositeData.Application.PartnerName}", actual.Message);
        }
    }
}
