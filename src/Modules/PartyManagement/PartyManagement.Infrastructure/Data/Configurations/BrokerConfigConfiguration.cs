using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class BrokerConfigConfiguration : IEntityTypeConfiguration<BrokerConfig>
    {
        public void Configure(EntityTypeBuilder<BrokerConfig> builder)
        {
            builder.ToTable("BrokerConfig", "Party");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // PartyId - FK to PartyMaster (required)
            builder.Property(m => m.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(m => m.Party)
                .WithMany(t => t.BrokerConfigs)
                .HasForeignKey(m => m.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            // SettlementCycleId - same-module FK to Party.MiscMaster
            builder.Property(m => m.SettlementCycleId)
                .HasColumnName("SettlementCycleId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.HasOne(m => m.SettlementCycleMisc)
                .WithMany(t => t.BrokerConfigSettlementCycle)
                .HasForeignKey(m => m.SettlementCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // TdsApplicable - bit
            builder.Property(m => m.TdsApplicable)
                .HasColumnName("TdsApplicable")
                .HasColumnType("tinyint")
                .IsRequired();

            // TdsCode
            builder.Property(m => m.TdsCode)
                .HasColumnName("TdsCode")
                .HasColumnType("nvarchar(10)")
                .IsRequired(false);

            // DefaultCommissionGl
            builder.Property(m => m.DefaultCommissionGl)
                .HasColumnName("DefaultCommissionGl")
                .HasColumnType("nvarchar(10)")
                .IsRequired(false);

            // AgreementStartDate
            builder.Property(m => m.AgreementStartDate)
                .HasColumnName("AgreementStartDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // AgreementEndDate
            builder.Property(m => m.AgreementEndDate)
                .HasColumnName("AgreementEndDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // BrokerPayableControlGl
            builder.Property(m => m.BrokerPayableControlGl)
                .HasColumnName("BrokerPayableControlGl")
                .HasColumnType("nvarchar(10)")
                .IsRequired(false);

            // TargetAmount
            builder.Property(m => m.TargetAmount)
                .HasColumnName("TargetAmount")
                .HasColumnType("decimal(15,3)")
                .IsRequired(false);

            // TargetPeriod
            builder.Property(m => m.TargetPeriod)
                .HasColumnName("TargetPeriod")
                .HasColumnType("nvarchar(10)")
                .IsRequired(false);

            // Status - byte
            builder.Property(m => m.Status)
                .HasColumnName("Status")
                .HasColumnType("tinyint")
                .IsRequired();

            // Indexes
            builder.HasIndex(m => m.PartyId);
        }
    }
}
