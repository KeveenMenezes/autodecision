using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class ApplicationFlagsInternalMessageMap : IEntityTypeConfiguration<ApplicationFlagsInternalMessage>
	{
		public void Configure(EntityTypeBuilder<ApplicationFlagsInternalMessage> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.MessageTypeId)
                .HasMaxLength(2);
            entity.Property(e => e.Code)
                .HasMaxLength(5);
            entity.Property(e => e.ProcessedAt);
			entity.Property(e => e.IsDeleted);
			
			entity.Property(e => e.Message)
				.HasColumnType("varchar(1000)");


			
		}
	}
}
