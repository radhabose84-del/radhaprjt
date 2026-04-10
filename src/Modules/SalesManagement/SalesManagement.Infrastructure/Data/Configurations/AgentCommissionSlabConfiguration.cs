using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class AgentCommissionSlabConfiguration : IEntityTypeConfiguration<AgentCommissionSlab>
    {
        public void Configure(EntityTypeBuilder<AgentCommissionSlab> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("AgentCommissionSlab", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentCommissionConfigId)
                .HasColumnName("AgentCommissionConfigId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SlabOrder)
                .HasColumnName("SlabOrder")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FromDelay)
                .HasColumnName("FromDelay")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ToDelay)
                .HasColumnName("ToDelay")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.CommissionTypeId)
                .HasColumnName("CommissionTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CommissionBasisId)
                .HasColumnName("CommissionBasisId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CommissionValue)
                .HasColumnName("CommissionValue")
                .HasColumnType("decimal(18,4)")
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
            builder.HasIndex(t => t.AgentCommissionConfigId);
            builder.HasIndex(t => new { t.AgentCommissionConfigId, t.SlabOrder }).IsUnique();
            builder.HasIndex(t => t.CommissionTypeId);
            builder.HasIndex(t => t.CommissionBasisId);

            // FK: AgentCommissionSlab → AgentCommissionConfig (parent)
            builder.HasOne(t => t.AgentCommissionConfig)
                .WithMany(a => a.AgentCommissionSlabs)
                .HasForeignKey(t => t.AgentCommissionConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: AgentCommissionSlab → MiscMaster for CommissionType (slab-level)
            builder.HasOne(t => t.CommissionType)
                .WithMany(m => m.AgentCommissionSlabsAsCommissionType)
                .HasForeignKey(t => t.CommissionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: AgentCommissionSlab → MiscMaster for CommissionBasis (slab-level)
            builder.HasOne(t => t.CommissionBasis)
                .WithMany(m => m.AgentCommissionSlabsAsCommissionBasis)
                .HasForeignKey(t => t.CommissionBasisId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
