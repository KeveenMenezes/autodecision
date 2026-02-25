using Microsoft.EntityFrameworkCore;
using AutodecisionCore.Data.Mappings;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Models.Trigger;

namespace AutodecisionCore.Data.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public virtual DbSet<HealthCheck> HealthChecks { get; set; }
        public virtual DbSet<Flag> Flags { get; set; }
		public virtual DbSet<AutoApprovalPaymentType> AutoApprovalPaymentTypes { get; set; }
		public virtual DbSet<AutoApprovalFundingMethod> AutoApprovalFundingMethods { get; set; }
		public virtual DbSet<ApplicationCore> ApplicationCores { get; set; }
		public virtual DbSet<ApplicationFlag> ApplicationFlags { get; set; }
		public virtual DbSet<ApplicationProcess> ApplicationProcesses { get; set; }
		public virtual DbSet<AutoApprovalRule> AutoApprovalRules { get; set; }
		public virtual DbSet<ApplicationFlagsInternalMessage> ApplicationFlagsInternalMessages { get; set; }
		public virtual DbSet<AutoApprovalUwCluster> AutoApprovalUwClusters { get; set; }
		public virtual DbSet<Allotment> Allotments { get; set; }
        public virtual DbSet<Trigger> Triggers { get; set; }
        public virtual DbSet<TriggerFlag> TriggerFlags { get; set; }
        public virtual DbSet<TimePeriod> TimePeriods { get; set; }
        public virtual DbSet<DeclineReasonFlags> DeclineReasonFlags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new HealthCheckMap());
			modelBuilder.ApplyConfiguration(new ApplicationCoreMap());
			modelBuilder.ApplyConfiguration(new FlagMap());
			modelBuilder.ApplyConfiguration(new ApplicationFlagsInternalMessageMap());
			modelBuilder.ApplyConfiguration(new ApplicationFlagMap());
			modelBuilder.ApplyConfiguration(new ApplicationProcessMap());
			modelBuilder.ApplyConfiguration(new AutoApprovalRuleMap());
			modelBuilder.ApplyConfiguration(new AutoApprovalUwClusterMap());
			modelBuilder.ApplyConfiguration(new AllotmentMap());
            modelBuilder.ApplyConfiguration(new TriggerMap());
            modelBuilder.ApplyConfiguration(new TriggerFlagMap());
            modelBuilder.ApplyConfiguration(new TimePeriodMap());
            modelBuilder.ApplyConfiguration(new DeclineReasonFlagsMap());
        }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}
