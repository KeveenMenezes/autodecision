using System.ComponentModel;

namespace AutodecisionCore.Contracts.Enums;

public enum StatusIncome
{
    [Description("Pending")]
    Pending = 1,

    [Description("Approved")]
    Approved = 2,

    [Description("Reproved")]
    Reproved = 3
}