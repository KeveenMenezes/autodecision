using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories;
using AutodecisionCore.Services.Base;

namespace AutodecisionCore.Services
{
    public interface IHealthCheckService : IBaseService<HealthCheck> { }

    public class HealthCheckService : BaseService<HealthCheck>, IHealthCheckService
    {
        public HealthCheckService(IHealthCheckRepository repository) : base(repository) { }
    }
}
