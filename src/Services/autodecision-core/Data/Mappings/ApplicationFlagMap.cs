using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Data.Mappings
{
    public class ApplicationFlagMap : IEntityTypeConfiguration<ApplicationFlag>
    {
        public void Configure(EntityTypeBuilder<ApplicationFlag> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.ApprovedAt)
                .IsRequired(false);
            entity.Property(e => e.ApprovedBy)
                .IsRequired(false);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.ProcessedAt)
                .IsRequired(false);
            entity.Property(e => e.RequestedAt)
                .IsRequired(false); ;
            entity.Property(e => e.Status)
                .HasMaxLength(2);

			entity.Property(e => e.FlagCode)
                .IsRequired()
                .HasColumnType("varchar(4)");
            entity.Property(e => e.Description)
                .IsRequired(false)
                .HasColumnType("varchar(1000)");
            entity.Property(e => e.ApprovalNote)
                .IsRequired(false)
                .HasColumnType("varchar(1000)");
            entity.Property(e => e.ApprovedByName)
                .IsRequired(false)
                .HasColumnType("varchar(1000)");


            var nav = entity.Metadata
                .FindNavigation(nameof(ApplicationFlag.ApplicationFlagsInternalMessage));
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
