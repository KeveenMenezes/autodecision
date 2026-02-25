using System.Collections.Generic;
using AutodecisionCore.Contracts.Enums;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class TotalIncome
{
    public int ApplicationId { get; set; }
    public string? PayFrequency { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TotalAmountGross { get; set; }
    public StatusIncome Status { get; set; }
    public string? StatusDescription { get; set; }
    public List<Income> Incomes { get; set; } = [];
}
