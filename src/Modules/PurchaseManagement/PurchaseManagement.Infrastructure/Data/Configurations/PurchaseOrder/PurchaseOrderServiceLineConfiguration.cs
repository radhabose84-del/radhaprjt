using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class PurchaseOrderServiceLineConfiguration : IEntityTypeConfiguration<PurchaseOrderServiceLine>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderServiceLine> b)
        {
                 b.ToTable("PurchaseOrderServiceLine", "Purchase");

            b.HasKey(x => x.Id);

            // required
            b.Property(x => x.PurchaseOrderId).IsRequired();
            b.Property(x => x.ServicePoHeaderId).IsRequired();   // FK to header
            b.Property(x => x.LineNo).IsRequired();

            // optionals
            b.Property(x => x.RequestId);
            b.Property(x => x.ServiceId);
            b.Property(x => x.ServiceDescription).HasMaxLength(500).IsUnicode(true);
            b.Property(x => x.UOMId);

            // numbers
            b.Property(x => x.PlannedQuantity).HasPrecision(18, 3);
            b.Property(x => x.PlannedRate).HasPrecision(18, 2);
            b.Property(x => x.PlannedValue).HasPrecision(18, 2);
            b.Property(x => x.DiscountId).HasPrecision(18, 2);            
            b.Property(x => x.Discount).HasPrecision(18, 2);
            b.Property(x => x.ItemCost).HasPrecision(18, 2);
            b.Property(x => x.OtherCost).HasPrecision(18, 2);
            b.Property(x => x.OtherCharges).HasPrecision(18, 2);
            b.Property(x => x.GstPercent).HasPrecision(5, 2);
            b.Property(x => x.Remarks).HasMaxLength(500).IsUnicode(true);

            //  line → service header (this was missing)
            b.HasOne(l => l.ServicePoHeader)
             .WithMany(h => h.Items)                // make sure header has: ICollection<PurchaseOrderServiceLine> Items { get; set; }
             .HasForeignKey(l => l.ServicePoHeaderId)
             .OnDelete(DeleteBehavior.Cascade);

            // line → schedules
            b.HasMany(x => x.PurchaseOrderServiceSchedules)
             .WithOne()
             .HasForeignKey(s => s.ServiceItemId)
             .OnDelete(DeleteBehavior.Cascade);            
           

        }
    }
}