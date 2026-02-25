namespace AutodecisionCore.Contracts.ViewModels.Application;

public class FactorTrust
{
    public int? RiskScore { get; set; }
    public int? AddressChangesLastTwoYears { get; set; }
    public decimal? BalanceToIncome { get; set; }
    public string Mla { get; set; }
    public string TcaNoHit { get; set; }
    public string TcaDeceased { get; set; }
    public string TcaAlert { get; set; }
    public string TcaActiveMilitaryDutyAlert { get; set; }
    public string TcaConsStmtText { get; set; }
    public string TcaTrueNameFraudText { get; set; }
    public string TcaSecurityAlertText { get; set; }
    public string TcaInitialFraudAlertText { get; set; }
    public string TcaExtendedFraudAlertText { get; set; }
    public string TcaActiveMilitaryDutyAlertText { get; set; }
    public string TcaAlertDetails { get; set; }
    public bool? HasFactorTrustInconsistency { get; set; }
}
