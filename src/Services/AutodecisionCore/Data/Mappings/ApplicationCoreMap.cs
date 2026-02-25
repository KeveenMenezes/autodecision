using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class ApplicationCoreMap : IEntityTypeConfiguration<ApplicationCore>
    {
        public void Configure(EntityTypeBuilder<ApplicationCore> entity)
        {
            entity.Property(e => e.Id);
            entity.Property(e => e.ProcessingVersion)
                .HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasMaxLength(2).HasConversion(
                v => (int)v,
                v => (InternalStatusEnum)v);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.LoanNumber)
                .IsRequired()
                .HasColumnType("varchar(20)");
            entity.Property(e => e.CustomerName)
                .IsRequired(false)
                .HasColumnType("varchar(100)");
            entity.Property(e => e.EmployerName)
                .IsRequired(false)
                .HasColumnType("varchar(100)");
            entity.Property(e => e.StateAbbreviation)
                .IsRequired(false)
                .HasColumnType("varchar(3)");
            entity.Property(e => e.ProcessedAt);
			entity.Property(e => e.Type)
                .IsRequired(false)
				.HasColumnType("varchar(1)");

			var nav = entity.Metadata
                .FindNavigation(nameof(ApplicationCore.ApplicationFlags));
            nav.SetPropertyAccessMode(PropertyAccessMode.Field);

            var nav2 = entity.Metadata
                .FindNavigation(nameof(ApplicationCore.ApplicationProcesses));
            nav2.SetPropertyAccessMode(PropertyAccessMode.Field);

            var nav3 = entity.Metadata
                .FindNavigation(nameof(ApplicationCore.AutoApprovalRules));
            nav3.SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.HasIndex(x => x.LoanNumber)
                .IsUnique(false)
                .HasDatabaseName("IX_ApplicationsCore_LoanNumber");
        }
    }
}
