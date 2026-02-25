using System;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class BlockList
{
    public string Reason { get; set; }
    public string CreatedNote { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
}
