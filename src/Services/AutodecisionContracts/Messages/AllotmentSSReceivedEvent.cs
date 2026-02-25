
namespace AutodecisionCore.Contracts.Messages;

public class AllotmentSddReceivedEvent
{
    public string LoanNumber { get; set; }

    public string ReconciliationSystem { get; set; }

    public string RoutingNumber { get; set; }

    public string AccountNumber { get; set; }

    public decimal Value { get; set; }
}
