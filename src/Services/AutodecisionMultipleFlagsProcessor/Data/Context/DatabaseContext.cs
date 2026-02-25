using Microsoft.EntityFrameworkCore;
using AutodecisionMultipleFlagsProcessor.Data.Mappings;
using AutodecisionMultipleFlagsProcessor.Data.Models;

namespace AutodecisionMultipleFlagsProcessor.Data.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public virtual DbSet<HealthCheck> HealthChecks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new HealthCheckMap());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}
