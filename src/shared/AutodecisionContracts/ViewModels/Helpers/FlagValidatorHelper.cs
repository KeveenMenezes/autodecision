using System;
using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.Helpers;

public class FlagValidatorHelper
{
    public CustomerSkipAutoDeny? CustomerSkipAutoDeny { get; set; }
    public bool EmployerAllowAutoDeny { get; set; }
    public List<ApplicationDocuments> ApplicationDocuments { get; set; } = new List<ApplicationDocuments>();
    public List<ApplicationSkipStep> ApplicationSkipSteps { get; set; } = new List<ApplicationSkipStep>();
}

public class CustomerSkipAutoDeny
{
    public bool Active { get; set; }
    public DateTime? ActivatedAt { get; set; }
}

public class ApplicationDocuments
{
    public string DocumentType { get; set; }
    public bool Uploaded { get; set; }
    public string? ReviewStatus { get; set; }
}

public class ApplicationSkipStep
{
    public string StepKey { get; set; }
    public long? VendorId { get; set; }
    public bool Active { get; set; }
}