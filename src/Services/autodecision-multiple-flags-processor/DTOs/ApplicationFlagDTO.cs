using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionMultipleFlagsProcessor.DTOs
{
        public class ApplicationFlagDTO
        {
            public ApplicationFlag Data { get; set; }
        }

        public class ApplicationFlag
        {
            public int id { get; set; }
            public int flag_id { get; set; }
            public int application_id { get; set; }
            public string description { get; set; }
            public string status { get; set; }
            public DateTime? approved_at { get; set; }
            public int? approved_by { get; set; }
            public string note { get; set; }
            public DateTime created_at { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string user_name { get; set; }
    }
}
