using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace Infrastructure.Persistence.Configurations;

public class QuotationHeaderConfiguration : IEntityTypeConfiguration<QuotationHeader>
{
    public void Configure(EntityTypeBuilder<QuotationHeader> b)
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
        
        b.ToTable("QuotationHeader", schema: "Purchase");
        b.HasKey(x => x.Id);

        // Required props
        b.Property(x => x.QuotationNumber).HasMaxLength(40).IsRequired();
        b.Property(x => x.ValidTill).IsRequired();
        b.Property(x => x.SupplierId).IsRequired();

        b.Property(x => x.RfqId).IsRequired();
        b.HasOne(m => m.Rfq)
               .WithMany(g => g.QuotationRfq)
               .HasForeignKey(m => m.RfqId)
               .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.FreightModeId).IsRequired(false);
        b.Property(x => x.PaymentTermsId).IsRequired(false);
        b.Property(x => x.IncotermsId).IsRequired(false);        

        // Money/Totals
        b.Property(x => x.Freight).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxableSubtotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.GstTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.ItemsTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsuranceCharge).HasColumnType("decimal(18,2)");

        // Optional header image path
        b.Property(x => x.QuotationImage).HasMaxLength(500);
        
        // Freight Mode
        b.HasOne(x => x.MiscFreightMode)
         .WithMany(m => m.QuotationFreightMode)
         .HasForeignKey(x => x.FreightModeId)
         .OnDelete(DeleteBehavior.NoAction)
         .HasConstraintName("FK_QuotationHeader_FreightMode");

        // Payment Terms (add the collection QuotationPaymentTerms in MiscMaster)
        b.HasOne(x => x.MiscPaymentTerms)
         .WithMany(m => m.QuotationPaymentTerms)
         .HasForeignKey(x => x.PaymentTermsId)
         .OnDelete(DeleteBehavior.NoAction)
         .HasConstraintName("FK_QuotationHeader_PaymentTerms");

        // Incoterms
        b.HasOne(x => x.MiscIncoterms)
         .WithMany(m => m.QuotationIncoterms)
         .HasForeignKey(x => x.IncotermsId)
         .OnDelete(DeleteBehavior.NoAction)
         .HasConstraintName("FK_QuotationHeader_Incoterms");
        
        b.HasMany(x => x.Lines)
         .WithOne(x => x.Header)
         .HasForeignKey(x => x.QuotationHeaderId)
         .OnDelete(DeleteBehavior.Cascade);

        // Unique per Supplier + RFQ (soft delete-aware)
        b.HasIndex(x => new { x.SupplierId, x.RfqId, x.IsDeleted })
         .IsUnique()
         .HasFilter("[IsDeleted] = 0")
         .HasDatabaseName("IX_UQ_Quotation_Supplier_Rfq_NotDeleted");

        // QuotationNumber index with includes
        b.HasIndex(x => x.QuotationNumber)
         .HasDatabaseName("IX_Purchase_QuotationHeader_QuotationNumber")
         .IncludeProperties(h => new { h.SupplierId, h.RfqId, h.ValidTill });
         
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

