using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class DeclineReasonFlagsMap : IEntityTypeConfiguration<DeclineReasonFlags>
    {
        public void Configure(EntityTypeBuilder<DeclineReasonFlags> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.FlagCode)
                .IsRequired()
                .HasColumnType("varchar(4)");
            entity.Property(e => e.ReasonId);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.IsDeleted);
        }
    }
}
