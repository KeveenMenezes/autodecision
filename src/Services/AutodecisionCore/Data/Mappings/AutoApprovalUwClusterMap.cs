using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
	public class AutoApprovalUwClusterMap : IEntityTypeConfiguration<AutoApprovalUwCluster>
	{
		public void Configure(EntityTypeBuilder<AutoApprovalUwCluster> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt).IsRequired();
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.UwCluster).IsRequired();
			entity.Property(e => e.IsAllowed).IsRequired();
			entity.ToTable("auto_approval_uwcluster");
		}
	}
}
