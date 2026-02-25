using AutodecisionCore.Data.Models.Trigger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class TriggerFlagMap : IEntityTypeConfiguration<TriggerFlag>
    {
        public void Configure(EntityTypeBuilder<TriggerFlag> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.FlagCode)
                .IsRequired()
                .HasColumnType("varchar(4)");
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.IsDeleted);
        }
    }
}
