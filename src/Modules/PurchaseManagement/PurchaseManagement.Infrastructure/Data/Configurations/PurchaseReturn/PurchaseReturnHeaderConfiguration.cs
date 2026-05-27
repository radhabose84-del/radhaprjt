using PurchaseManagement.Domain.Entities.PurchaseReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseReturn;

public class PurchaseReturnHeaderConfiguration : IEntityTypeConfiguration<PurchaseReturnHeader>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnHeader> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive);

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

        b.ToTable("PurchaseReturnHeader", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.RtvNumber).HasColumnType("varchar(40)").IsRequired();
        b.Property(x => x.RtvDate).IsRequired();
        b.Property(x => x.UnitId).IsRequired();
        b.Property(x => x.VendorId).IsRequired();
        b.Property(x => x.PoId).IsRequired();
        b.Property(x => x.GrnHeaderId).IsRequired();
        b.Property(x => x.ReturnTypeId).IsRequired();
        b.Property(x => x.ReturnReasonId).IsRequired();
        b.Property(x => x.ReturnActionId).IsRequired();
        b.Property(x => x.IsReplacementRequired).HasColumnType("bit").IsRequired();
        b.Property(x => x.IsDebitNoteRequired).HasColumnType("bit").IsRequired();
        b.Property(x => x.IsQcVerified).HasColumnType("bit").IsRequired();
        b.Property(x => x.Remarks).HasColumnType("varchar(500)").IsRequired(false);
        b.Property(x => x.StatusId).IsRequired();
        b.Property(x => x.ApprovalRequestId).IsRequired(false);
        b.Property(x => x.ReplacementStatusId).IsRequired(false);
        b.Property(x => x.ReplacementClosedDate).HasColumnType("datetimeoffset").IsRequired(false);

        b.HasOne(x => x.Po)
         .WithMany()
         .HasForeignKey(x => x.PoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.GrnHeader)
         .WithMany()
         .HasForeignKey(x => x.GrnHeaderId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReturnType)
         .WithMany(t => t.PurchaseReturns)
         .HasForeignKey(x => x.ReturnTypeId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReturnReason)
         .WithMany(r => r.PurchaseReturns)
         .HasForeignKey(x => x.ReturnReasonId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReturnAction)
         .WithMany()
         .HasForeignKey(x => x.ReturnActionId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.MiscStatus)
         .WithMany()
         .HasForeignKey(x => x.StatusId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReplacementStatus)
         .WithMany()
         .HasForeignKey(x => x.ReplacementStatusId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.RtvNumber).IsUnique().HasDatabaseName("UQ_PurchaseReturnHeader_RtvNumber");
        b.HasIndex(x => new { x.UnitId, x.IsDeleted }).HasDatabaseName("IX_PurchaseReturnHeader_Unit_NotDeleted");
        b.HasIndex(x => x.VendorId).HasDatabaseName("IX_PurchaseReturnHeader_VendorId");
        b.HasIndex(x => x.PoId).HasDatabaseName("IX_PurchaseReturnHeader_PoId");
        b.HasIndex(x => x.GrnHeaderId).HasDatabaseName("IX_PurchaseReturnHeader_GrnHeaderId");
        b.HasIndex(x => x.StatusId).HasDatabaseName("IX_PurchaseReturnHeader_StatusId");

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
