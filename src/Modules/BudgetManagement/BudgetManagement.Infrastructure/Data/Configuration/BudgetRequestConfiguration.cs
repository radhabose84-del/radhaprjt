using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.Infrastructure.Data.Configurations;

public sealed class BudgetRequestConfiguration : IEntityTypeConfiguration<BudgetRequest>
{
    public void Configure(EntityTypeBuilder<BudgetRequest> builder)
    {
        // ----------------------------
        // Enum converters (Status/IsDelete -> bit)
        // ----------------------------
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        // ----------------------------
        // DateOnly converters
        // ----------------------------
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d)
        );

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
            d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null
        );

        // ----------------------------
        // Table & Key
        // ----------------------------
        builder.ToTable("BudgetRequest", schema: "Budget");
        builder.HasKey(x => x.Id);

        // ----------------------------
        // Columns
        // ----------------------------
        builder.Property(x => x.UnitId).IsRequired();
        builder.Property(x => x.FinancialYearId).IsRequired();
        builder.Property(x => x.CurrencyId).IsRequired();

        builder.Property(x => x.RequestCode)
               .HasMaxLength(50);

        builder.Property(x => x.RequestAmount)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.Remarks)
               .HasMaxLength(300);

        builder.Property(x => x.ImagePath)
               .HasMaxLength(500);

        // Entity has DateOnly? so DO NOT force IsRequired() here.
        builder.Property(x => x.FromDate)
               .HasColumnType("date")
               .HasConversion(nullableDateOnlyConverter);

        builder.Property(x => x.ToDate)
               .HasColumnType("date")
               .HasConversion(nullableDateOnlyConverter);

        builder.Property(x => x.ProjectId)
               .HasColumnType("int");

       builder.Property(x => x.WBSId)
               .HasColumnType("int");

        builder.Property(x => x.BudgetGroupId)
               .HasColumnType("int");

        builder.Property(x => x.StatusId)
               .HasColumnType("int")
               .IsRequired();
       builder.Property(b => b.BudgetGroupId)
            .HasColumnName("BudgetGroupId")
            .HasColumnType("int")
            .IsRequired(false);
        // ----------------------------
        // Relationships to MiscMaster
        // ----------------------------

        // Request Type (OPEX/CAPEX)
        builder.HasOne(x => x.MiscRequestType)
               .WithMany(m => m.BudgetRequests)
               .HasForeignKey(x => x.RequestTypeId)
               .OnDelete(DeleteBehavior.NoAction);

        // Request By (who raised)
        builder.HasOne(x => x.MiscRequestBy)
               .WithMany(m => m.BudgetRequestBy)
               .HasForeignKey(x => x.RequestById)
               .OnDelete(DeleteBehavior.NoAction);

        // Request Month (month drop-down from Misc)
        builder.HasOne(x => x.MiscRequestMonth)
               .WithMany(m => m.BudgetRequestMonth)
               .HasForeignKey(x => x.RequestMonthId)
               .OnDelete(DeleteBehavior.NoAction);

        // BudgetGroup relationship (optional)
        builder.HasOne(x => x.BudgetGroupType)
               .WithMany(t => t.BudgetRequestGroupType)
               .HasForeignKey(x => x.BudgetGroupId)
               .OnDelete(DeleteBehavior.Restrict);

        // ----------------------------
        // Global query filter (soft delete)
        // ----------------------------
        builder.HasQueryFilter(x => x.IsDeleted == IsDelete.NotDeleted);

        // ----------------------------
        // BaseEntity mappings
        // ----------------------------
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

        builder.Property(b => b.CreatedBy)
               .HasColumnType("int")
               .IsRequired();

        builder.Property(b => b.CreatedDate)
               .HasColumnType("datetimeoffset");

        builder.Property(b => b.CreatedByName)
               .HasColumnType("varchar(50)");

        builder.Property(b => b.CreatedIP)
               .HasColumnType("varchar(20)");

        builder.Property(b => b.ModifiedBy)
               .HasColumnType("int");

        builder.Property(b => b.ModifiedDate)
               .HasColumnType("datetimeoffset");

        builder.Property(b => b.ModifiedByName)
               .HasColumnType("varchar(50)");

        builder.Property(b => b.ModifiedIP)
               .HasColumnType("varchar(20)");

        // ----------------------------
        // Business rule enforcement:
        // CAPEX: ProjectId NOT NULL -> BudgetGroupId MUST be NULL
        // OPEX : ProjectId NULL     -> BudgetGroupId MUST be NOT NULL
        // ----------------------------
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_BudgetRequest_Project_vs_BudgetGroup",
                "([ProjectId] IS NOT NULL AND [BudgetGroupId] IS NULL) OR ([ProjectId] IS NULL AND [BudgetGroupId] IS NOT NULL)"
            );
        });

        // ----------------------------
        // Unique indexes (Filtered)
        // ----------------------------

        // OPEX uniqueness (BudgetGroup based)
        builder.HasIndex(x => new
            {
                x.UnitId,
                x.FinancialYearId,
                x.RequestTypeId,
                x.BudgetGroupId,
                x.FromDate,
                x.ToDate,
                x.RequestById
            })
            .IsUnique()
            .HasDatabaseName("UX_BudgetRequest_OPEX_Uniq")
            .HasFilter("[ProjectId] IS NULL AND [BudgetGroupId] IS NOT NULL");

        // CAPEX uniqueness (Project based)
        builder.HasIndex(x => new
            {
                x.UnitId,
                x.FinancialYearId,
                x.RequestTypeId,
                x.ProjectId,
                x.WBSId,
                x.FromDate,
                x.ToDate,
                x.RequestById
            })
            .IsUnique()
            .HasDatabaseName("UX_BudgetRequest_CAPEX_Uniq")
            .HasFilter("[ProjectId] IS NOT NULL");
    }
}
