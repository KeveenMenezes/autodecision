using System;

namespace AutodecisionCore.Contracts.ViewModels.OpenPayrollData;

public class PayAllocations
{
    public DateTime CreatedAt { get; set; }

    public decimal? Value { get; set; }

    public bool IsRemainder { get; set; }

    public string RoutingNumber { get; set; }

    public string AccountNumber { get; set; }

    public string AccountType { get; set; }
}
