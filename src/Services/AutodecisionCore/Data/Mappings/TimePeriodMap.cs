using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class TimePeriodMap : IEntityTypeConfiguration<TimePeriod>
    {
        public void Configure(EntityTypeBuilder<TimePeriod> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.Description).HasColumnType("varchar(50)");
            entity.Property(e => e.Interval);
            entity.Property(e => e.UnitTime);
            entity.Property(e => e.IsDefault);
        }
    }
}
