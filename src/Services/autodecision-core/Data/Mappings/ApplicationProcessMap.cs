using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class ApplicationProcessMap : IEntityTypeConfiguration<ApplicationProcess>
	{
		public void Configure(EntityTypeBuilder<ApplicationProcess> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt);
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.ProcessedAt);
			entity.Property(e => e.ProcessingVersion)
                .HasMaxLength(10);
			
			entity.Property(e => e.Status)
                .HasMaxLength(2)
                .HasConversion(
				v => (int)v,
				v => (InternalStatusEnum)v
			);

			
		}
	}
}
