using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintResolutionConfiguration : IEntityTypeConfiguration<ComplaintResolution>
    {
        public void Configure(EntityTypeBuilder<ComplaintResolution> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ComplaintResolution", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintHeaderId).HasColumnName("ComplaintHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ResolutionTypeId).HasColumnName("ResolutionTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ResolutionSummary).HasColumnName("ResolutionSummary").HasColumnType("nvarchar(2000)").IsRequired();

            // Sales Return fields
            builder.Property(t => t.ReturnQuantity).HasColumnName("ReturnQuantity").HasColumnType("decimal(18,4)").IsRequired(false);
            builder.Property(t => t.ReturnLocationId).HasColumnName("ReturnLocationId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ReturnStatusId).HasColumnName("ReturnStatusId").HasColumnType("int").IsRequired(false);

            // Credit Note fields
            builder.Property(t => t.CreditAmount).HasColumnName("CreditAmount").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.FinanceReference).HasColumnName("FinanceReference").HasColumnType("varchar(100)").IsRequired(false);

            // Replacement fields
            builder.Property(t => t.ReplacementQuantity).HasColumnName("ReplacementQuantity").HasColumnType("decimal(18,4)").IsRequired(false);
            builder.Property(t => t.DispatchReference).HasColumnName("DispatchReference").HasColumnType("varchar(100)").IsRequired(false);

            // Reprocess fields
            builder.Property(t => t.ActionDescription).HasColumnName("ActionDescription").HasColumnType("nvarchar(2000)").IsRequired(false);

            // Closure fields
            builder.Property(t => t.ClosureStatusId).HasColumnName("ClosureStatusId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ClosureRemarks).HasColumnName("ClosureRemarks").HasColumnType("nvarchar(2000)").IsRequired(false);

            // Audit fields
            builder.Property(t => t.ResolvedBy).HasColumnName("ResolvedBy").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ResolvedDate).HasColumnName("ResolvedDate").IsRequired(false);
            builder.Property(t => t.ClosedBy).HasColumnName("ClosedBy").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ClosedDate).HasColumnName("ClosedDate").IsRequired(false);

            // Base entity fields
            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // FK → ComplaintHeader (one resolution per complaint)
            builder.HasOne(t => t.ComplaintHeader)
                .WithMany()
                .HasForeignKey(t => t.ComplaintHeaderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // FK → MiscMaster (ResolutionType)
            builder.HasOne(t => t.ResolutionType)
                .WithMany()
                .HasForeignKey(t => t.ResolutionTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ReturnLocationId is a CROSS-MODULE FK → Warehouse.WarehouseMaster (no DB constraint).
            // Validated at the application boundary via IWarehouseLookup. No HasOne mapping here.

            // FK → MiscMaster (ReturnStatus)
            builder.HasOne(t => t.ReturnStatus)
                .WithMany()
                .HasForeignKey(t => t.ReturnStatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // FK → MiscMaster (ClosureStatus)
            builder.HasOne(t => t.ClosureStatus)
                .WithMany()
                .HasForeignKey(t => t.ClosureStatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.ComplaintHeaderId).IsUnique();
            builder.HasIndex(t => t.ResolutionTypeId);
            builder.HasIndex(t => t.ClosureStatusId);
        }
    }
}
