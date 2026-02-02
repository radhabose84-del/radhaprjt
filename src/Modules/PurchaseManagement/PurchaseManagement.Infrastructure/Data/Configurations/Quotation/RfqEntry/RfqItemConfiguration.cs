using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Quotation.RfqEntry;

public class RfqItemConfiguration : IEntityTypeConfiguration<RfqItem>
{
    public void Configure(EntityTypeBuilder<RfqItem> b)
    {
        b.ToTable("RfqItem", "Purchase");
        b.HasKey(x => x.Id);
        b.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

        b.Property(x => x.ItemId).IsRequired();
        b.Property(x => x.HsnId).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        b.Property(x => x.UomId).IsRequired();        

        // Prevent duplicate lines inside the same RFQ
        b.HasIndex(x => new { x.RfqId, x.ItemId, x.UomId}).IsUnique();
    }
}
