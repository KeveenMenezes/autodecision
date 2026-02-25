using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class FlagCriteriaDTO
    {

        public List<FlagCriteria> Data { get; set; }

    }
    public class FlagCriteria
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string rule_type { get; set; }
        public string param_type { get; set; }
        public string trigger { get; set; }
        public string class_name { get; set; }
        public int? level { get; set; }
        public string active { get; set; }
        public DateTime? created_at { get; set; }
        public int? created_by { get; set; }
        public int employer_id { get; set; }
        public string param_value { get; set; }
        public string override_type { get; set; }
        public string status { get; set; }
        public bool turndown_active { get; set; }
        public string active_employer_flag { get; set; }
    }
}
