using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.DTOs;
using Microsoft.Extensions.Logging;
using Moq;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class PhoneValidationHandlerTests
    {
        private readonly Mock<ILogger<PhoneValidationHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public PhoneValidationHandlerTests()
        {
            _mockLogger = new Mock<ILogger<PhoneValidationHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void GivenApplicationTypeNotNewLoanWhenProcessingFlagThenFlagResultEqualsIgnored()
        {
            // Arrange
            var phoneValidationHandler = new PhoneValidationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.Refi,
                    LoanNumber = "test"
                }
            };
            var expected = FlagResultEnum.Ignored;

            // Act
            var actual = phoneValidationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenOtherCustomersEmptyListWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var phoneValidationHandler = new PhoneValidationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var emptyList = new List<SimilarPhoneDataDTO>() { };
            _mockCustomerInfo.Setup(mock => mock.GetCustomersWithSamePhone(1, "123", "231", "312")).ReturnsAsync(emptyList);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1,
                    PhoneNumber = "123",
                    SecondaryPhoneNumber = "231",
                    WorkPhoneNumber = "312"
                }
            };
            var expected = FlagResultEnum.Processed;

            // Act
            var actual = phoneValidationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(actual.FlagResult, expected);
        }

        [Fact]
        public void GivenOthersCustomersListHasDataWhenProcessingFlagThenFlagResultEqualsProcessed()
        {
            // Arrange
            var phoneValidationHandler = new PhoneValidationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var otherCustomers = new List<SimilarPhoneDataDTO>()
            {
                new SimilarPhoneDataDTO("123", "Foo", "Bar"),
                new SimilarPhoneDataDTO("321", "Bar", "Foo")
            };
            _mockCustomerInfo.Setup(mock => mock.GetCustomersWithSamePhone(1, "123", "231", "312")).ReturnsAsync(otherCustomers);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1,
                    PhoneNumber = "123",
                    SecondaryPhoneNumber = "231",
                    WorkPhoneNumber = "312"
                }
            };
            var expected = FlagResultEnum.PendingApproval;

            // Act
            var actual = phoneValidationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.Equal(expected, actual.FlagResult);
        }

        [Fact]
        public void GivenOthersCustomersListHasDataWhenProcessingFlagThenFlagMessageIsNotNull()
        {
            // Arrange
            var phoneValidationHandler = new PhoneValidationHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);
            var otherCustomers = new List<SimilarPhoneDataDTO>()
            {
                new SimilarPhoneDataDTO("123", "Foo", "Bar"),
                new SimilarPhoneDataDTO("321", "Bar", "Foo")
            };
            _mockCustomerInfo.Setup(mock => mock.GetCustomersWithSamePhone(1, "123", "231", "312")).ReturnsAsync(otherCustomers);
            var mockAutodecisionCompositeData = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "test"
                },
                Customer = new Customer()
                {
                    Id = 1,
                    PhoneNumber = "123",
                    SecondaryPhoneNumber = "231",
                    WorkPhoneNumber = "312"
                }
            };

            // Act
            var actual = phoneValidationHandler.ProcessFlag(mockAutodecisionCompositeData);

            // Assert
            Assert.NotNull(actual.Message);
            Assert.NotEmpty(actual.Message);
        }

    }
}