using System;

namespace AutodecisionCore.Contracts.ViewModels.OpenPayrollData;

public class Payouts
{
    public DateTime PayDate { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public string EmployerName { get; set; }
}
