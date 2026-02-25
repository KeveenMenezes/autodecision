using AutodecisionCore.Contracts.Enums;
using System;
using System.Collections.Generic;

namespace AutodecisionCore.Contracts.Messages;

public class ProcessFlagResponseEvent
{
    public string LoanNumber { get; set; }

    public string FlagCode { get; set; }

    public string Message { get; set; }

    public List<InternalMessage>? InternalMessages { get; set; }

    public FlagResultEnum FlagResult { get; set; }

    public DateTime ProcessedAt { get; set; }

    public int? Version { get; set; }

    public string ApprovalNote { get; set; }

    public string Reason { get; set; }

}
