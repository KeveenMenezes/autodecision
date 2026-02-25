namespace AutodecisionCore.Utils
{
    public static class AllotmentRuleConstants
    {
        public const string PaymentTypeNotAllotment = "Application payment type is not Allotment/SDD.";
        public const string AllotmentFoundNeeded = "Allotment/SDD found as needed.";
        public const string InvalidJPMorganAccount = "Invalid JP Morgan Account.";
        public const string ReconciliationChanged = "Reconciliation system or Value has changed.";
        public const string ProgramChanged = "Program has changed. Need to request Allotment/SDD.";
        public const string FirstLoan = "First Loan. Need to request Allotment/SDD.";
        public const string OpenPayrollMandatory = "Open payroll is mandatory and customer is not connected";
    }

    public static class DueDiligenceRuleConstants
    {
        public const string DueDiligenceRejected = "Due diligence rejected or under review";
    }
}
