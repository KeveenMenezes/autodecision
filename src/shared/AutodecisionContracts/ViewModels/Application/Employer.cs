namespace AutodecisionCore.Contracts.ViewModels.Application;

public class Employer
{
    public int Id { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string Program { get; set; } = string.Empty;
    public int? SubProgramId { get; set; }
    public int? DueDiligenceStatus { get; set; }

}
