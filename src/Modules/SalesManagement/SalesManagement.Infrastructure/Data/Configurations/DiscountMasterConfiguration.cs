using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DiscountMasterConfiguration : IEntityTypeConfiguration<DiscountMaster>
    {
        public void Configure(EntityTypeBuilder<DiscountMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DiscountMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DiscountCode)
                .HasColumnName("DiscountCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.DiscountName)
                .HasColumnName("DiscountName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.TriggerEventId)
                .HasColumnName("TriggerEventId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DiscountBasisId)
                .HasColumnName("DiscountBasisId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ExecutionTypeId)
                .HasColumnName("ExecutionTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CurrencyId)
                .HasColumnName("CurrencyId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.CustomerGroupId)
                .HasColumnName("CustomerGroupId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Priority)
                .HasColumnName("Priority")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RequiresApproval)
                .HasColumnName("RequiresApproval")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.MaxDiscountLimitTypeId)
                .HasColumnName("MaxDiscountLimitTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.MaxDiscountValue)
                .HasColumnName("MaxDiscountValue")
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(t => t.IsStackable)
                .HasColumnName("IsStackable")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.ExclusionGroupId)
                .HasColumnName("ExclusionGroupId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ValueTypeId)
                .HasColumnName("ValueTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SlabTypeId)
                .HasColumnName("SlabTypeId")
                .HasColumnType("int")
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
            builder.HasIndex(t => t.DiscountCode).IsUnique();
            builder.HasIndex(t => t.TriggerEventId);
            builder.HasIndex(t => t.DiscountBasisId);
            builder.HasIndex(t => t.ExecutionTypeId);
            builder.HasIndex(t => t.CurrencyId);
            builder.HasIndex(t => t.CustomerGroupId);
            builder.HasIndex(t => t.MaxDiscountLimitTypeId);
            builder.HasIndex(t => t.ExclusionGroupId);
            builder.HasIndex(t => t.ValueTypeId);
            builder.HasIndex(t => t.SlabTypeId);

            // Same-module FK — MiscMaster for TriggerEvent
            builder.HasOne(t => t.TriggerEvent)
                .WithMany(m => m.DiscountMastersAsTriggerEvent)
                .HasForeignKey(t => t.TriggerEventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for DiscountBasis
            builder.HasOne(t => t.DiscountBasis)
                .WithMany(m => m.DiscountMastersAsDiscountBasis)
                .HasForeignKey(t => t.DiscountBasisId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for ExecutionType
            builder.HasOne(t => t.ExecutionType)
                .WithMany(m => m.DiscountMastersAsExecutionType)
                .HasForeignKey(t => t.ExecutionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for CustomerGroup (optional)
            builder.HasOne(t => t.CustomerGroup)
                .WithMany(m => m.DiscountMastersAsCustomerGroup)
                .HasForeignKey(t => t.CustomerGroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for MaxDiscountLimitType (optional)
            builder.HasOne(t => t.MaxDiscountLimitType)
                .WithMany(m => m.DiscountMastersAsMaxDiscountLimitType)
                .HasForeignKey(t => t.MaxDiscountLimitTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for ExclusionGroup (optional)
            builder.HasOne(t => t.ExclusionGroup)
                .WithMany(m => m.DiscountMastersAsExclusionGroup)
                .HasForeignKey(t => t.ExclusionGroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for ValueType
            builder.HasOne(t => t.ValueType)
                .WithMany(m => m.DiscountMastersAsValueType)
                .HasForeignKey(t => t.ValueTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — MiscMaster for SlabType
            builder.HasOne(t => t.SlabType)
                .WithMany(m => m.DiscountMastersAsSlabType)
                .HasForeignKey(t => t.SlabTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CurrencyId — cross-module FK (UserManagement), no DB constraint
            // Validated in code via ICurrencyLookup
        }
    }
}
