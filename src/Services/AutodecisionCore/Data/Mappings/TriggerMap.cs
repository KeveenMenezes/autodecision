using AutodecisionCore.Data.Models.Trigger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class TriggerMap : IEntityTypeConfiguration<Trigger>
    {
        public void Configure(EntityTypeBuilder<Trigger> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasColumnType("varchar(1000)");
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.IsDeleted);

            var nav = entity.Metadata
                .FindNavigation(nameof(Trigger.Flags));
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
