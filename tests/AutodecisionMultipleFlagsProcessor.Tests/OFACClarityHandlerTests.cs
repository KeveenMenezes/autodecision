using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OFACClarityHandlerTests
    {
        private readonly Mock<ILogger<OFACClarityHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public OFACClarityHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OFACClarityHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenClarityIsNull_ShouldReturnIgnored()
        {
            // Arrange
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    Id = 12345
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "5434534534"
                },
                Clarity = null
            };

            var handler = new OFACClarityHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            // Act
            var response = handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(FlagResultEnum.Ignored, response.FlagResult);
            Assert.Equal("Customer not found on clarity.", response.Message);
        }

        [Fact]
        public void WhenClarityOFACHitIsTrue_ShouldReturnPendingApproval()
        {
            // Arrange
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    Id = 12345
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "5434534534"
                },
                Clarity = new Clarity
                {
                    OFACHit = true
                }
            };

            var handler = new OFACClarityHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            // Act
            var response = handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(FlagResultEnum.PendingApproval, response.FlagResult);
            Assert.Equal("This customer appears on the OFAC sanctions list.", response.Message);
            
            // Verify that RaiseFlag was called
            _mockFlagHelper.Verify(x => x.RaiseFlag(It.IsAny<ProcessFlagResponseEvent>(), 
                "This customer appears on the OFAC sanctions list."), Times.Once);
        }

        [Fact]
        public void WhenClarityOFACHitIsFalse_ShouldReturnApproved()
        {
            // Arrange
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    Id = 12345
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "5434534534"
                },
                Clarity = new Clarity
                {
                    OFACHit = false
                }
            };

            var handler = new OFACClarityHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            // Act
            var response = handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(FlagResultEnum.Approved, response.FlagResult);
            Assert.Equal("No match found on the OFAC sanctions list for this customer.", response.Message);
            
            // Verify that RaiseFlag was NOT called
            _mockFlagHelper.Verify(x => x.RaiseFlag(It.IsAny<ProcessFlagResponseEvent>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    Id = 12345
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "LOAN123"
                },
                Clarity = new Clarity
                {
                    OFACHit = false
                }
            };

            // Setup mock to throw exception when BuildFlagResponse is called
            _mockFlagHelper.Setup(x => x.BuildFlagResponse(It.IsAny<string>(), 
                It.IsAny<AutodecisionCompositeData>(), It.IsAny<FlagResultEnum>()))
                .Throws(new Exception("Test exception"));

            var handler = new OFACClarityHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            // Act
            var response = handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(FlagResultEnum.Error, response.FlagResult);
            Assert.Equal("Error processing OFAC Clarity flag.", response.Message);
        }

        [Fact]
        public void WhenClarityOFACHitIsNull_ShouldReturnApproved()
        {
            // Arrange
            var obj = new AutodecisionCompositeData()
            {
                Customer = new Customer
                {
                    Id = 12345
                },
                Application = new Application()
                {
                    Status = "2",
                    Type = ApplicationType.NewLoan,
                    LoanNumber = "LOAN123"
                },
                Clarity = new Clarity
                {
                    // OFACHit not defined and defaults to false
                }
            };

            var handler = new OFACClarityHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            // Act
            var response = handler.ProcessFlag(obj);

            // Assert
            Assert.Equal(FlagResultEnum.Approved, response.FlagResult);
            Assert.Equal("No match found on the OFAC sanctions list for this customer.", response.Message);
        }
    }
}
