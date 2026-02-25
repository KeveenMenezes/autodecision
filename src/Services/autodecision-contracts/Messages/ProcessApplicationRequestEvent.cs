using System;

namespace AutodecisionCore.Contracts.Messages;

public class ProcessApplicationRequestEvent
{
    public int ApplicationId { get; set; }
    public string LoanNumber { get; set; }
    public string Reason { get; set; }
    public DateTime RequestedAt { get; set; }
}
