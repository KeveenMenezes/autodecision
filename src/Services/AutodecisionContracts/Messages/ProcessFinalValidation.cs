namespace AutodecisionCore.Contracts.Messages;

public class ProcessFinalValidation
{
    public string LoanNumber { get; set; }
    public string Key { get; set; }
    public int? Version { get; set; }
    public string Reason { get; set; }
}
