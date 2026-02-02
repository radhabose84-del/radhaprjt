using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Persistence.Configurations;

public class QuotationDetailConfiguration : IEntityTypeConfiguration<QuotationDetail>
{
    public void Configure(EntityTypeBuilder<QuotationDetail> b)
    {
           // --- enum -> bit converters (unchanged) ---
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );
        
        b.ToTable("QuotationDetail", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.ItemId).IsRequired();        
        b.Property(x => x.HsnId).IsRequired();   
        b.Property(x => x.UomId).IsRequired();
        b.Property(x => x.CurrencyId).IsRequired();

        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.Rate).HasColumnType("decimal(18,2)");
        b.Property(x => x.Discount).HasColumnType("decimal(18,2)").IsRequired(false);
        b.Property(x => x.GstPercent).HasColumnType("decimal(5,2)");
        b.Property(x => x.Warranty).HasColumnType("decimal(18,2)").IsRequired(false);        
        b.Property(x => x.ValidityDays).HasColumnType("decimal(18,2)").IsRequired(false);
        b.Property(x => x.DeliveryDays).HasColumnType("decimal(18,2)").IsRequired(false);        

        // Computed/snapshots
        b.Property(x => x.LineSubtotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.GstAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Total).HasColumnType("decimal(18,2)");
        b.Property(x => x.PandFCharge).HasPrecision(18, 2);
        
        b.HasOne(x => x.MiscQuoDiscountType)
         .WithMany(m => m.QuotationDetailDiscount)
         .HasForeignKey(x => x.DiscountTypeId)
         .OnDelete(DeleteBehavior.NoAction);

        // Optional: prevent duplicate Item per quotation (line-level uniqueness)
        b.HasIndex(x => new { x.QuotationHeaderId, x.ItemId, x.IsDeleted })
            .HasDatabaseName("IX_QuotationDetail_Header_Item_NotDeleted")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

           // --- BaseEntity columns (unchanged) ---
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
