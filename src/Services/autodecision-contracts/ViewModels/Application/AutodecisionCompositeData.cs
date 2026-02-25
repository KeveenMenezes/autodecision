using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class AutodecisionCompositeData
{
    public Customer Customer { get; set; }

    public Application Application { get; set; }

    public List<LastApplication> LastApplications { get; set; }

    public OpenPayroll OpenPayroll { get; set; }

    public OpenBanking OpenBanking { get; set; }

    public CreditPolicy CreditPolicy { get; set; }

    public Census Census { get; set; }

    public BlockList BlockList { get; set; }

    public WhiteList WhiteList { get; set; }

    public List<ApplicationWarning> ApplicationWarnings { get; set; }

    public TransunionResult TransunionResult { get; set; }

    public FactorTrust FactorTrust { get; set; }

    public Clarity Clarity { get; set; }

    public DebitCard DebitCard { get; set; }

    public FaceRecognition FaceRecognition { get; set; }

    public FlagValidatorHelper FlagValidatorHelper { get; set; }

    public CreditRisk CreditRisk { get; set; }

    public Employer Employer { get; set; }

    public TotalIncome TotalIncome { get; set; }

    public int? Version { get; set; }

}