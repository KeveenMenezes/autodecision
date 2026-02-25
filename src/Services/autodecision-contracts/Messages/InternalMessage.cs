using AutodecisionCore.Contracts.Enums;

namespace AutodecisionCore.Contracts.Messages;

public class InternalMessage
{
    public InternalMessageType MessageType { get; set; }

    public int Code { get; set; }

    public string Message { get; set; }
}
