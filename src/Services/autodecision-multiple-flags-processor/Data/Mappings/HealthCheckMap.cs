using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutodecisionMultipleFlagsProcessor.Data.Models;

namespace AutodecisionMultipleFlagsProcessor.Data.Mappings
{
    public partial class HealthCheckMap : IEntityTypeConfiguration<HealthCheck>
    {
        public void Configure(EntityTypeBuilder<HealthCheck> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(100).IsRequired();
            entity.ToTable("health_checks");
        }
    }
}
