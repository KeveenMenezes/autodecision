namespace AutodecisionCore.Contracts.ViewModels.Application;

public class CreditPolicyEntity
{
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int EmployerId { get; set; }
    public int ProductId { get; set; }
    public string StateAbbreviation { get; set; }
    public int PricingId { get; set; }
    public int LoanAmountTableId { get; set; }
    public int GroupNewId { get; set; }
    public int GroupRefiId { get; set; }
    public int PaymentsInARowId { get; set; }
    public decimal? LocNew { get; set; }
    public decimal? LocRefi { get; set; }
    public int? LoeNew { get; set; }
    public int? LoeRefi { get; set; }
}
