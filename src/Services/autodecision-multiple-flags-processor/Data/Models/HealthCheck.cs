using AutodecisionMultipleFlagsProcessor.Data.Models.Base;

namespace AutodecisionMultipleFlagsProcessor.Data.Models
{
    public class HealthCheck : BaseModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
