namespace AutodecisionCore.Contracts.ViewModels.Application;

public class ApplicationConnections
{
    public int ApplicationId { get; set; }
    public int? OpenBankingId { get; set; }
    public int? OpenPayrollId { get; set; }
    public long? FaceId { get; set; }
    public int? DebitCardId { get; set; }
}
