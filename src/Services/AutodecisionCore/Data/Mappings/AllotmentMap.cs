using AutodecisionCore.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutodecisionCore.Data.Mappings
{
	public class AllotmentMap : IEntityTypeConfiguration<Allotment>
	{
		public void Configure(EntityTypeBuilder<Allotment> entity)
		{
			entity.Property(e => e.Id);
			entity.Property(e => e.CreatedAt);
			entity.Property(e => e.IsDeleted);
			entity.Property(e => e.LoanNumber).HasColumnType("varchar(50)");
			entity.Property(e => e.ReconciliationSystem).HasColumnType("varchar(50)");
			entity.Property(e => e.RoutingNumber).HasColumnType("varchar(50)");
			entity.Property(e => e.AccountNumber).HasColumnType("varchar(50)");
			entity.Property(e => e.Value);
		}
	}
}
