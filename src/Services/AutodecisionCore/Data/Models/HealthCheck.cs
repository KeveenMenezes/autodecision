using AutodecisionCore.Data.Models.Base;

namespace AutodecisionCore.Data.Models
{
    public class HealthCheck : BaseModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
