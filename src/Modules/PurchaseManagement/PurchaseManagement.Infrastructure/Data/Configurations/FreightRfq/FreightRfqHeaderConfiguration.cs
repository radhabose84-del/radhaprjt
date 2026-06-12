using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.FreightRfq;

public class FreightRfqHeaderConfiguration : IEntityTypeConfiguration<FreightRfqHeader>
{
    public void Configure(EntityTypeBuilder<FreightRfqHeader> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("FreightRfqHeader", schema: "Purchase");
        b.HasKey(x => x.Id);

        // Identity
        b.Property(x => x.FreightRfqNumber).HasColumnType("varchar(20)").IsRequired();
        b.Property(x => x.RfqDate).HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.RfqValidTill).HasColumnType("datetimeoffset").IsRequired(false);

        // Type & PO linkage
        b.Property(x => x.RfqTypeId).IsRequired();
        b.Property(x => x.PoReferenceId).IsRequired(false);
        b.Property(x => x.SupplierId).IsRequired(false);

        // Route
        b.Property(x => x.SourceLocation).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.SourceStation).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.DestinationLocation).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.DestinationStation).HasColumnType("varchar(100)").IsRequired();

        // Freight calculation basis
        b.Property(x => x.TotalQuantity).HasColumnType("decimal(18,3)").IsRequired();
        b.Property(x => x.TotalBaleCount).IsRequired();

        // Approval
        b.Property(x => x.StatusId).IsRequired();
        b.Property(x => x.SelectedQuotationId).IsRequired(false);
        b.Property(x => x.ComparisonRemarks).HasColumnType("varchar(500)").IsRequired(false);
        b.Property(x => x.ApprovedTransporterId).IsRequired(false);
        b.Property(x => x.ApprovedRate).HasColumnType("decimal(18,2)").IsRequired(false);
        b.Property(x => x.ApprovedFreightValue).HasColumnType("decimal(18,2)").IsRequired(false);
        b.Property(x => x.ApprovalRemarks).HasColumnType("varchar(500)").IsRequired(false);

        // FK: RFQ Type -> Purchase.MiscMaster (same module, no inverse navigation)
        b.HasOne<MiscMaster>()
         .WithMany()
         .HasForeignKey(x => x.RfqTypeId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_FreightRfqHeader_RfqType");

        // FK: Status -> Purchase.MiscMaster (same module, no inverse navigation)
        b.HasOne<MiscMaster>()
         .WithMany()
         .HasForeignKey(x => x.StatusId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_FreightRfqHeader_Status");

        // FK: PO Reference -> Purchase.RawMaterialPOHeader (same module, nullable)
        b.HasOne<RawMaterialPOHeader>()
         .WithMany()
         .HasForeignKey(x => x.PoReferenceId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_FreightRfqHeader_RawMaterialPO");

        // SupplierId, SelectedQuotationId, ApprovedTransporterId -> no DB FK (cross-module / self-cycle)

        // Children
        b.HasMany(x => x.Quotations)
         .WithOne(x => x.Header)
         .HasForeignKey(x => x.FreightRfqHeaderId)
         .OnDelete(DeleteBehavior.Cascade);

        // Unique RFQ number (soft delete-aware)
        b.HasIndex(x => new { x.FreightRfqNumber, x.IsDeleted })
         .IsUnique()
         .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("IX_UQ_FreightRfqHeader_Number_NotDeleted");

        b.HasIndex(x => x.PoReferenceId).HasDatabaseName("IX_FreightRfqHeader_PoReferenceId");
        b.HasIndex(x => x.StatusId).HasDatabaseName("IX_FreightRfqHeader_StatusId");

        // BaseEntity columns
        b.Property(x => x.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
        b.Property(x => x.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int").IsRequired();
        b.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int").IsRequired(false);
        b.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate").HasColumnType("datetimeoffset").IsRequired(false);
        b.Property(x => x.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(50)").IsRequired(false);
        b.Property(x => x.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)").IsRequired(false);
    }
}
