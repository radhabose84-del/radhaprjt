using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class PurchaseOrderServiceHeaderConfiguration : IEntityTypeConfiguration<PurchaseOrderServiceHeader>
    {

        public void Configure(EntityTypeBuilder<PurchaseOrderServiceHeader> b)
        {
            b.ToTable("PurchaseOrderServiceHeader", "Purchase");
            b.HasKey(x => x.Id);

            // Required fields
            b.Property(x => x.PurchaseOrderId).IsRequired();
            b.Property(x => x.ServiceCategoryId).IsRequired();


            // Optional fields (no IsRequired)
            b.Property(x => x.ContractTypeId);
            b.Property(x => x.FrequencyId);
            b.Property(x => x.ValidityFrom);
            b.Property(x => x.ValidityTo);
            b.Property(x => x.TotalOccurrences);

            b.Property(x => x.OverallLimit)
            .HasPrecision(18, 2);               // precision ok; nulls allowed because decimal?

            b.Property(x => x.TermsId);           

            b.Property(x => x.TermDescription)
            .IsUnicode(true)
            .HasColumnType("nvarchar(max)")     // nulls allowed because string?
            .IsRequired(false);                 // (optional but explicit)

            b.Property(x => x.CostCenterId);
            b.Property(x => x.ModeOfDispatchId);
            b.Property(x => x.FreightCharges);

            b.Property(x => x.DeliveryAddress).HasMaxLength(500);
            b.Property(x => x.BillingAddress).HasMaxLength(500);

            b.Property(x => x.POImage)
            .IsUnicode(true)
            .HasMaxLength(500)
            .IsRequired(false);                 // (optional but explicit) 

            b.HasOne(x => x.MiscModeOfDispatch)
            .WithMany(m => m.PurchaseServiceHeaderMode)
            .HasForeignKey(x => x.ModeOfDispatchId)
            .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.PurchaseOrder)
              .WithMany(m => m.ServicePos)
              .HasForeignKey(x => x.PurchaseOrderId)
              .OnDelete(DeleteBehavior.NoAction);

            // Relationships for MiscMaster lookups
            b.HasOne(x => x.MiscServiceCategory)  // Navigation to MiscMaster (ServiceCategory)
             .WithMany(m => m.PurchaseOrderServiceHeaderServiceCategories)
             .HasForeignKey(x => x.ServiceCategoryId)
              .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.MiscContractType)  // Navigation to MiscMaster (ContractType)
             .WithMany(m => m.PurchaseOrderServiceHeaderContractTypes)
             .HasForeignKey(x => x.ContractTypeId)
              .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.MiscFrequency)  // Navigation to MiscMaster (Frequency)
             .WithMany(m => m.PurchaseOrderServiceHeaderFrequencies)
             .HasForeignKey(x => x.FrequencyId)
            .OnDelete(DeleteBehavior.NoAction);

            // Relationships for PurchaseOrderServiceLine (1-to-many)
            b.HasMany(x => x.Items)
           .WithOne(l => l.ServicePoHeader)
           .HasForeignKey(l => l.ServicePoHeaderId)
           .OnDelete(DeleteBehavior.Cascade);
            
             // unique: 1 service-PO per PO
            b.HasIndex(x => x.PurchaseOrderId)
             .IsUnique()
             .HasDatabaseName("UX_PurchaseOrderServiceHeader_PurchaseOrderId");
         
        }
    }
}