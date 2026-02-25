using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Tests.AutodecisionCoreAggregate
{
    public class ApplicationCoreTests
    {
        [Fact]
        public void ApplicationCoreConstructorInitializesProperties()
        {
            // Arrange
            var loanNumber = "123456789";

            // Act
            var applicationCore = new ApplicationCore(loanNumber);

            // Assert
            Assert.Equal(loanNumber, applicationCore.LoanNumber);
            Assert.Equal(1, applicationCore.ProcessingVersion);
            Assert.Equal(InternalStatusEnum.Pending, applicationCore.Status);
        }

        //TODO: Create test cases for all remaining methods
    }
}