using AutodecisionMultipleFlagsProcessor.Data.Context;
using AutodecisionMultipleFlagsProcessor.Data.Models;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Base;

namespace AutodecisionMultipleFlagsProcessor.Data.Repositories
{
    public interface IHealthCheckRepository : IBaseRepository<HealthCheck> { }

    public class HealthCheckRepository : BaseRepository<HealthCheck>, IHealthCheckRepository
    {
        public HealthCheckRepository(DatabaseContext dbContext) : base(dbContext) { }
    }
}
