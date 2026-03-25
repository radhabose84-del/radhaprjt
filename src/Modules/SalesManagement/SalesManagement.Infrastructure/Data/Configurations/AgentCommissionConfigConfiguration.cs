using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class AgentCommissionConfigConfiguration : IEntityTypeConfiguration<AgentCommissionConfig>
    {
        public void Configure(EntityTypeBuilder<AgentCommissionConfig> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            // Table mapping (user-specified table name)
            builder.ToTable("AgentCommissionConfig", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentId)
                .HasColumnName("AgentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesSegmentId)
                .HasColumnName("SalesSegmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CommissionTypeId)
                .HasColumnName("CommissionTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CommissionBasisId)
                .HasColumnName("CommissionBasisId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ApplicableLevelId)
                .HasColumnName("ApplicableLevelId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.CommissionPercentage)
                .HasColumnName("CommissionPercentage")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.CurrencyId)
                .HasColumnName("CurrencyId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ValidityFrom)
                .HasColumnName("ValidityFrom")
                .IsRequired();

            builder.Property(t => t.ValidityTo)
                .HasColumnName("ValidityTo")
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.AgentId);
            builder.HasIndex(t => t.SalesSegmentId);
            builder.HasIndex(t => t.CommissionTypeId);
            builder.HasIndex(t => t.CommissionBasisId);
            builder.HasIndex(t => t.ApplicableLevelId);
            builder.HasIndex(t => new { t.ValidityFrom, t.ValidityTo });

            // Composite index for overlap query performance
            builder.HasIndex(t => new { t.AgentId, t.SalesSegmentId });

            // Same-module FK — SalesSegment (DB constraint)
            builder.HasOne(t => t.SalesSegment)
                .WithMany()
                .HasForeignKey(t => t.SalesSegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for CommissionType (DB constraint)
            builder.HasOne(t => t.MiscMaster)
                .WithMany()
                .HasForeignKey(t => t.CommissionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for CommissionBasis (DB constraint)
            builder.HasOne(t => t.CommissionBasis)
                .WithMany(m => m.AgentCommissionConfigsAsCommissionBasis)
                .HasForeignKey(t => t.CommissionBasisId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for ApplicableLevel (DB constraint)
            builder.HasOne(t => t.ApplicableLevel)
                .WithMany(m => m.AgentCommissionConfigsAsApplicableLevel)
                .HasForeignKey(t => t.ApplicableLevelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FKs (AgentId, CurrencyId) — NO DB FK constraints
        }
    }
}
