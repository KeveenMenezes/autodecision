namespace AutodecisionCore.Contracts.ViewModels.Application;

public class ApplicationScore
{
    public string Group { get; set; }
    public decimal Score { get; set; }
    public string ModelVersion { get; set; }
    public decimal CutOff { get; set; }
}
