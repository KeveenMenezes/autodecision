using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;

namespace AutodecisionCore.Data.Repositories
{
    public interface IHealthCheckRepository : IBaseRepository<HealthCheck> { }

    public class HealthCheckRepository : BaseRepository<HealthCheck>, IHealthCheckRepository
    {
        public HealthCheckRepository(DatabaseContext dbContext) : base(dbContext) { }
    }
}
