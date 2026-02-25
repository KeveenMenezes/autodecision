namespace AutodecisionCore.Contracts.Constants;

public class ApplicationWarningType
{
    public const string DuplicateSSN = "1";
    public const string LessThanOneYearAtJob = "2";
    public const string LessThan18Years = "3";
    public const string ActiveMilitary = "4";
    public const string AnotherState = "5";
    public const string DuplicateBankAccount = "6";
    public const string ApplicationPurposeBusiness = "7";
    public const string CustomerNotFoundOnCensus = "8";
    public const string InsufficientGrossPay = "9";
    public const string InvalidPayroll = "10";
    public const string NotLicensedState = "11";
}