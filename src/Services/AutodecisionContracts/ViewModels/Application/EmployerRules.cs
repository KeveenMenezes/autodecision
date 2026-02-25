using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class EmployerRules
{
    public List<EmployerRulesItem> EmployerRulesItems;
}

public class EmployerRulesItem
{
    public string Key { get; set; }

    public string ValueType { get; set; }

    public string Min { get; set; }

    public string Max { get; set; }

    public string Default { get; set; }

    public bool Required { get; set; }

}
