using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
	public class FlagMap : IEntityTypeConfiguration<Flag>
	{
		public void Configure(EntityTypeBuilder<Flag> entity)
		{
			entity.ToTable("flags");

			entity.HasKey(x => x.Id);

			entity.Property(x => x.Code)
				.IsRequired()
				.HasColumnType("varchar(10)");

			entity.Property(x => x.Name)
				.IsRequired()
				.HasColumnType("varchar(100)");

			entity.Property(x => x.Description)
				.HasColumnType("varchar(200)");

			entity.Property(x => x.Active)
				.IsRequired();

			entity.Property(e => e.IsWarning)
				.IsRequired()
				.HasDefaultValue(false);
		}
	}
}
