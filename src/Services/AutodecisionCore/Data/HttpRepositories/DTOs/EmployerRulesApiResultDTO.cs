namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class EmployerRulesApiResultDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
        public string GroupKey { get; set; }
        public string GroupName { get; set; }
        public string RuleKey { get; set; }
        public string RuleType { get; set; }
        public string ValueType { get; set; }
        public string DefaultValue { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public bool Required { get; set; }
        public bool Active { get; set; }
    }
}
