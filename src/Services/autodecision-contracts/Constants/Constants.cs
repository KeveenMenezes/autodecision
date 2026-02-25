namespace AutodecisionCore.Contracts.Constants;

public class Constants
{
    public class Topics
    {
        public const string Process = "autodecision-process";

        public const string FinalEvaluation = "autodecision-final-evaluation";

        public const string NotifyAprove = "autodecision-notify-approve";
        public const string NotifyDecline = "autodecision-notify-decline";
        public const string NotifyPending = "autodecision-notify-pending";
        public const string NotifyAllotment = "autodecision-notify-allotment";
        public const string NotifyAllotmentSddReceived = "autodecision-notify-allotment-sdd-received";
        public const string NotifyPendingDefaultDocuments = "autodecision-notify-default-documents";

        public const string RedisKeyPrefix = "autodecision-";

        public const string FlagResponse = "autodecision-flag-response";

        public const string FlagRequestPrefix = "autodecision-flag-";
    }

}
