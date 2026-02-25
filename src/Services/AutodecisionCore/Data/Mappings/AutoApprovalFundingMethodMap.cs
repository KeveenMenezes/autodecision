using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AutodecisionCore.Data.Mappings
{
	public class AutoApprovalFundingMethodMap : IEntityTypeConfiguration<AutoApprovalFundingMethod>
	{
		public void Configure(EntityTypeBuilder<AutoApprovalFundingMethod> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt).IsRequired();
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.FundingMethod).IsRequired();
			entity.Property(e => e.IsAllowed).IsRequired();
			entity.ToTable("auto_approval_funding_method");
		}
	}
}
