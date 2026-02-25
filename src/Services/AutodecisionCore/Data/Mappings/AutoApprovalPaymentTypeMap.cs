using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
	public class AutoApprovalPaymentTypeMap : IEntityTypeConfiguration<AutoApprovalPaymentType>
	{
		public void Configure(EntityTypeBuilder<AutoApprovalPaymentType> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt).IsRequired();
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.PaymentType).IsRequired();
			entity.Property(e => e.IsAllowed).IsRequired();
			entity.ToTable("auto_approval_application_type");
		}
	}
}
