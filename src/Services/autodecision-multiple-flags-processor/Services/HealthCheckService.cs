using AutodecisionMultipleFlagsProcessor.Data.Models;
using AutodecisionMultipleFlagsProcessor.Data.Repositories;
using AutodecisionMultipleFlagsProcessor.Services.Base;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public interface IHealthCheckService : IBaseService<HealthCheck> { }

    public class HealthCheckService : BaseService<HealthCheck>, IHealthCheckService
    {
        public HealthCheckService(IHealthCheckRepository repository) : base(repository) { }
    }
}
