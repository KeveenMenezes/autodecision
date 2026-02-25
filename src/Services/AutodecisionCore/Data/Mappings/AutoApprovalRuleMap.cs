using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
    public class AutoApprovalRuleMap : IEntityTypeConfiguration<AutoApprovalRule>
	{
		public void Configure(EntityTypeBuilder<AutoApprovalRule> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt);
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.Description)
				.IsRequired(false);
			
			entity.Property(e => e.RuleName)
				.HasColumnType("varchar(100)");
			
			entity.Property(e => e.Status).HasMaxLength(2).HasConversion(
				v => (int)v,
				v => (AutoApprovalResultEnum)v
			);

		}
	}
}
