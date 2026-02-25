using System;
using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class OpenBanking
{
    public List<OpenBankingConnections> Connections { get; set; }
}

public class OpenBankingConnections
{
    public string Vendor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string AccountNumber { get; set; }
    public string RoutingNumber { get; set; }
    public bool IsDefault { get; set; }
}