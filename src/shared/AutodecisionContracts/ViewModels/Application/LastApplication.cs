using System;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class LastApplication
{
    public int Id { get; set; }
    public string LoanNumber { get; set; }
    public decimal AmountOfPayment { get; set; }
    public string BrowserFingerprint { get; set; }
    public string BankRoutingNumber { get; set; }
    public string BankAccountNumber { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReconciliationSystem { get; set; }
    public string Program { get; set; }
    public ApplicationConnections? ApplicationConnections { get; set; }
    public bool HasCustomerIdentiyValidated { get; set; }
}
