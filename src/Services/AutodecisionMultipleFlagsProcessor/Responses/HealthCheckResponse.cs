using AutodecisionMultipleFlagsProcessor.Data.Models;

namespace AutodecisionMultipleFlagsProcessor.Contracts.Responses
{
    public class HealthCheckResponse
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public HealthCheckResponse() { }

        public HealthCheckResponse(HealthCheck hc)
        {
            Name = hc.Name;
            Version = hc.Version;
        }
    }
}
