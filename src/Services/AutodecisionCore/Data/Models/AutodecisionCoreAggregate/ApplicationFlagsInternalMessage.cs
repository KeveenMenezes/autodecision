using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Data.Models.Base;
using AutodecisionCore.Utils;
using Microsoft.VisualBasic;

namespace AutodecisionCore.Data.Models.AutodecisionCoreAggregate
{
    public class ApplicationFlagsInternalMessage : BaseModel
    {
        public string Message { get; set; }

        public int MessageTypeId { get; set; }

        public int Code { get; set; }

        public DateTime ProcessedAt { get; set; }

        public ApplicationFlagsInternalMessage(string message, int messageTypeId, int code)
        {
            Message = message;
            MessageTypeId = messageTypeId;
            Code = code;
            ProcessedAt = DateTimeUtil.Now;
        }
    }
}
