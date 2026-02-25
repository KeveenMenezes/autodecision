using AutodecisionCore.Data.Models;

namespace AutodecisionCore.Contracts.Responses
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
