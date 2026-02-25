using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.DTOs;

namespace AutodecisionCore.Utils;

public static class NewCreditPolicyMapper
{
    public static CreditPolicy MapToCreditPolicy(this CreditPolicyEntity creditPolicy, List<NewEmployerRulesDto> employerRules)
    {
        List<EmployerRulesItem> employerRulesItems = new();
        foreach(var er in employerRules)
        {
            employerRulesItems.Add(new EmployerRulesItem
            {
                  Key = er.Id.ToString(),
                  ValueType = er.ValueType,
                  Min = er.MinValue,
                  Max = er.MaxValue,
                  Required = true
            });
        }
        return new CreditPolicy
        {
            CreditPolicyEntity = creditPolicy,
            EmployerRules = new EmployerRules
            {
                EmployerRulesItems = employerRulesItems,
            }
        };      
    }
}