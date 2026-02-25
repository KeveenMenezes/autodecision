using System;
using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.OpenPayrollData;

public class OpenPayrollConnection
{
    public DateTime ConnectedAt { get; set; }
    public bool IsActive { get; set; }
    public ProfileInformation ProfileInformation { get; set; }
    public List<PayAllocations> PayAllocations { get; set; }
    public List<BmgAllotments> BmgAllotments { get; set; }
    public bool IsNewOpenPayroll { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime OldestPayDate { get; set; }
    public bool HasMoreThanOneEmployer { get; set; }
    public int VendorType { get; set; }
    public int? OcrolusDocumentStatus { get; set; }
    public int? OcrolusDocumentScore { get; set; }
    public List<OcrolusDocumentSignals>? OcrolusDocumentSignals { get; set; }
}