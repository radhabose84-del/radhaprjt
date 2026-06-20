using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class ProfitCentreConfiguration : IEntityTypeConfiguration<ProfitCentre>
    {
        public void Configure(EntityTypeBuilder<ProfitCentre> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ProfitCentre", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProfitCentreCode)
                .HasColumnName("ProfitCentreCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.ProfitCentreName)
                .HasColumnName("ProfitCentreName")
                .HasColumnType("varchar(150)")
                .IsRequired();

            builder.Property(t => t.LevelId)
                .HasColumnName("LevelId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ParentProfitCentreId)
                .HasColumnName("ParentProfitCentreId")
                .HasColumnType("int");

            builder.Property(t => t.ResponsibleHeadId)
                .HasColumnName("ResponsibleHeadId")
                .HasColumnType("int");

            builder.Property(t => t.IsRevenueLinked)
                .HasColumnName("IsRevenueLinked")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.MidYearJustification)
                .HasColumnName("MidYearJustification")
                .HasColumnType("varchar(500)");

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
            builder.HasOne(t => t.ParentProfitCentre)
                .WithMany(t => t.ChildProfitCentres)
                .HasForeignKey(t => t.ParentProfitCentreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> Finance.MiscMaster (level). Parameterless WithMany() — no reverse
            // collection on MiscMaster (matches the CostCentre CentreLevel FK style).
            builder.HasOne(t => t.LevelMaster)
                .WithMany()
                .HasForeignKey(t => t.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Code is unique ACROSS all companies (group segment reporting, AC#2) — NOT scoped by company.
            builder.HasIndex(t => t.ProfitCentreCode).IsUnique();
            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => t.ParentProfitCentreId);
            builder.HasIndex(t => t.LevelId);

            // Cross-module FKs (CompanyId, ResponsibleHeadId) intentionally have NO DB constraint —
            // resolved via lookup interfaces (ICompanyLookup / IUserLookup, CLAUDE.md Rule #3).
        }
    }
}
