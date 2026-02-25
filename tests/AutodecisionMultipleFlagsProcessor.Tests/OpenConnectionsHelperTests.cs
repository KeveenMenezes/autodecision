using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Helpers;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class OpenConnectionsHelperTests
    {
        [Fact]
        public void GetDaysFromLatestOpenBankingReturnsNullWhenConnectionsListIsNull()
        {
            List<OpenBankingConnections> connections = null;

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenBankingConnection(connections);

            Assert.Null(result);
        }

        [Fact]
        public void GetDaysFromLatestOpenBankingReturnsNullWhenConnectionsListIsEmpty()
        {
            List<OpenBankingConnections> connections = new List<OpenBankingConnections>();

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenBankingConnection(connections);

            Assert.Null(result);
        }

        [Fact]
        public void GetDaysFromLatestOpenBankingConnectionReturnsDaysDifferenceFromCreatedAt()
        {
            var currentDate = DateTime.Today;
            var connection1 = new OpenBankingConnections { CreatedAt = currentDate.Subtract(TimeSpan.FromDays(5)), UpdatedAt = currentDate.Subtract(TimeSpan.FromDays(4)) };
            var connection2 = new OpenBankingConnections { CreatedAt = currentDate.Subtract(TimeSpan.FromDays(3)), UpdatedAt = currentDate.Subtract(TimeSpan.FromDays(2)) };
            var connection3 = new OpenBankingConnections { CreatedAt = currentDate.Subtract(TimeSpan.FromDays(1)) };
            var connections = new List<OpenBankingConnections> { connection1, connection2, connection3 };

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenBankingConnection(connections);

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetDaysFromLatestOpenBankingConnectionReturnsDaysDifferenceFromUpdatedAt()
        {
            var currentDate = DateTime.Today;
            var connection1 = new OpenBankingConnections { CreatedAt = currentDate.Subtract(TimeSpan.FromDays(5)), UpdatedAt = currentDate.Subtract(TimeSpan.FromDays(4)) };
            var connection2 = new OpenBankingConnections { CreatedAt = currentDate.Subtract(TimeSpan.FromDays(3)), UpdatedAt = currentDate.Subtract(TimeSpan.FromDays(2)) };
            var connections = new List<OpenBankingConnections> { connection1, connection2 };

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenBankingConnection(connections);

            Assert.Equal(2, result);
        }

        [Fact]
        public void GetDaysFromLatestOpenPayrollConnectionReturnsNullWhenConnectionsListIsNull()
        {
            List<OpenPayrollConnection> connections = null;

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenPayrollConnection(connections);

            Assert.Null(result);
        }

        [Fact]
        public void GetDaysFromLatestOpenPayrollConnectionReturnsNullWhenConnectionsListIsEmpty()
        {
            List<OpenPayrollConnection> connections = new List<OpenPayrollConnection>();

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenPayrollConnection(connections);

            Assert.Null(result);
        }

        [Fact]
        public void GetDaysFromLatestOpenPayrollConnectionReturnsCorrectNumberOfDays()
        {
            var currentDate = DateTime.Today;
            var connection1 = new OpenPayrollConnection { ConnectedAt = currentDate.Subtract(TimeSpan.FromDays(5)) };
            var connection2 = new OpenPayrollConnection { ConnectedAt = currentDate.Subtract(TimeSpan.FromDays(3)) };
            var connection3 = new OpenPayrollConnection { ConnectedAt = currentDate.Subtract(TimeSpan.FromDays(2)) };
            var connections = new List<OpenPayrollConnection> { connection1, connection2, connection3 };

            var result = OpenConnectionsHelper.GetDaysFromLatestOpenPayrollConnection(connections);

            Assert.Equal(2, result);
        }
    }
}
