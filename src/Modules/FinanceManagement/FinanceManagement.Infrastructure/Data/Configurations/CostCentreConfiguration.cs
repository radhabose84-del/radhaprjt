using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class CostCentreConfiguration : IEntityTypeConfiguration<CostCentre>
    {
        public void Configure(EntityTypeBuilder<CostCentre> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CostCentre", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CostCentreCode)
                .HasColumnName("CostCentreCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.CostCentreName)
                .HasColumnName("CostCentreName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.CentreLevelId)
                .HasColumnName("CentreLevelId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ParentCostCentreId)
                .HasColumnName("ParentCostCentreId")
                .HasColumnType("int");

            builder.Property(t => t.DepartmentGroupId)
                .HasColumnName("DepartmentGroupId")
                .HasColumnType("int");

            builder.Property(t => t.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int");

            builder.Property(t => t.ResponsibleManagerId)
                .HasColumnName("ResponsibleManagerId")
                .HasColumnType("int");

            builder.Property(t => t.EffectiveFromDate)
                .HasColumnName("EffectiveFromDate");

            builder.Property(t => t.EffectiveToDate)
                .HasColumnName("EffectiveToDate");

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

            // Same-module self-reference (the parent one level up). Restrict — never cascade-delete the tree.
            builder.HasOne(t => t.ParentCostCentre)
                .WithMany(t => t.ChildCostCentres)
                .HasForeignKey(t => t.ParentCostCentreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> Finance.MiscMaster (level). Parameterless WithMany() — no reverse
            // collection on MiscMaster (matches the CurrencyTypeConfig FK style on GlAccountMaster).
            builder.HasOne(t => t.CentreLevelMaster)
                .WithMany()
                .HasForeignKey(t => t.CentreLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Code is unique within a unit; the same code may exist in another unit.
            builder.HasIndex(t => new { t.UnitId, t.CostCentreCode }).IsUnique();
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.ParentCostCentreId);
            builder.HasIndex(t => t.CentreLevelId);
            builder.HasIndex(t => t.DepartmentGroupId);
            builder.HasIndex(t => t.DepartmentId);

            // Cross-module FKs (UnitId, CompanyId, DepartmentGroupId, DepartmentId, ResponsibleManagerId)
            // intentionally have NO DB constraint — resolved via lookup interfaces (CLAUDE.md Rule #3).
        }
    }
}
