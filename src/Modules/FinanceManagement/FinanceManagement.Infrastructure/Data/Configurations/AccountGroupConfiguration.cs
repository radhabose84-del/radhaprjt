using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class AccountGroupConfiguration : IEntityTypeConfiguration<AccountGroup>
    {
        public void Configure(EntityTypeBuilder<AccountGroup> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("AccountGroup", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // Cross-module (UserManagement.Company) — column + index only, no DB FK constraint.
            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GroupCode)
                .HasColumnName("GroupCode")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.GroupName)
                .HasColumnName("GroupName")
                .HasColumnType("varchar(150)")
                .IsRequired();

            builder.Property(t => t.AccountTypeId)
                .HasColumnName("AccountTypeId")
                .HasColumnType("int");

            builder.Property(t => t.ScheduleIIILineItemId)
                .HasColumnName("ScheduleIIILineItemId")
                .HasColumnType("int");

            builder.Property(t => t.ParentAccountGroupId)
                .HasColumnName("ParentAccountGroupId")
                .HasColumnType("int");

            builder.Property(t => t.Level)
                .HasColumnName("Level")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.IsLeaf)
                .HasColumnName("IsLeaf")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.SortOrder)
                .HasColumnName("SortOrder")
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // GroupCode is globally unique across the hierarchy.
            builder.HasIndex(t => t.GroupCode).IsUnique();

            // Filtering / roll-up traversal.
            builder.HasIndex(t => t.ParentAccountGroupId);
            builder.HasIndex(t => t.Level);
            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => t.AccountTypeId);
            builder.HasIndex(t => t.ScheduleIIILineItemId);

            // Self-referencing single-parent FK (same-module). Restrict prevents deleting a
            // parent that still has children at the database level.
            builder.HasOne(t => t.ParentAccountGroup)
                .WithMany(p => p.Children)
                .HasForeignKey(t => t.ParentAccountGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Statutory head FK (same-module → Finance.AccountTypeMaster). Level 1 only; Restrict
            // so an account type cannot be deleted while a Level 1 group references it.
            builder.HasOne(t => t.AccountType)
                .WithMany()
                .HasForeignKey(t => t.AccountTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Schedule III line mapping FK (same-module → Finance.ScheduleIIILineItem). Nullable;
            // Restrict so a statutory line in use cannot be deleted.
            builder.HasOne(t => t.ScheduleIIILineItem)
                .WithMany()
                .HasForeignKey(t => t.ScheduleIIILineItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
