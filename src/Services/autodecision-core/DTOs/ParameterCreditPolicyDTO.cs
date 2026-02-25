namespace AutodecisionCore.DTOs;

public class ParameterCreditPolicyDTO
{
    public int Id { get; set; }
    public string ParameterKey { get; set; }
    public decimal? DecimalValue { get; set; }
    public int? IntValue { get; set; }
    public string? VarcharValue { get; set; }
}
