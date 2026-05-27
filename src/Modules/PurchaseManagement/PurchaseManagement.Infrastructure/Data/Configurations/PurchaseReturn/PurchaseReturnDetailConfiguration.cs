using PurchaseManagement.Domain.Entities.PurchaseReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseReturn;

public class PurchaseReturnDetailConfiguration : IEntityTypeConfiguration<PurchaseReturnDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnDetail> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive);

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

        b.ToTable("PurchaseReturnDetail", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.PurchaseReturnHeaderId).IsRequired();
        b.Property(x => x.GrnDetailId).IsRequired();
        b.Property(x => x.ItemId).IsRequired();
        b.Property(x => x.UomId).IsRequired();

        b.Property(x => x.ReceivedQty).HasColumnType("decimal(18,3)").IsRequired();
        b.Property(x => x.AcceptedQty).HasColumnType("decimal(18,3)").IsRequired();
        b.Property(x => x.ReturnQty).HasColumnType("decimal(18,3)").IsRequired();
        b.Property(x => x.RatePerUnit).HasColumnType("decimal(18,4)").IsRequired(false);
        b.Property(x => x.LineValue).HasColumnType("decimal(18,2)").IsRequired(false);
        b.Property(x => x.ReturnReasonId).IsRequired(false);
        b.Property(x => x.LineRemarks).HasColumnType("varchar(300)").IsRequired(false);

        b.HasOne(x => x.PurchaseReturnHeader)
         .WithMany(h => h.Details)
         .HasForeignKey(x => x.PurchaseReturnHeaderId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.GrnDetail)
         .WithMany()
         .HasForeignKey(x => x.GrnDetailId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ReturnReason)
         .WithMany(r => r.PurchaseReturnDetailReasons)
         .HasForeignKey(x => x.ReturnReasonId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.PurchaseReturnHeaderId).HasDatabaseName("IX_PurchaseReturnDetail_PurchaseReturnHeaderId");
        b.HasIndex(x => x.GrnDetailId).HasDatabaseName("IX_PurchaseReturnDetail_GrnDetailId");
        b.HasIndex(x => x.ItemId).HasDatabaseName("IX_PurchaseReturnDetail_ItemId");

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
