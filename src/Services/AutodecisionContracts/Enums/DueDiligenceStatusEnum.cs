using System.ComponentModel;

namespace AutodecisionCore.Contracts.Enums
{
    public enum DueDiligenceStatusEnum
    {
        [Description("Rejected")]
        Rejected = 1,

        [Description("Approved")]
        Approved = 2,

        [Description("UnderReview")]
        UnderReview = 3
    }
}
