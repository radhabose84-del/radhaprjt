using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Quotation.RfqEntry;

public class RfqSupplierConfiguration : IEntityTypeConfiguration<RfqSupplier>
{
    public void Configure(EntityTypeBuilder<RfqSupplier> b)
    {        
        b.ToTable("RfqSuppliers", "Purchase");
        b.HasKey(x => x.Id);
        b.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

        b.Property(x => x.Name).HasMaxLength(200).IsRequired(false);
        b.Property(x => x.GSTNumber).HasMaxLength(20).IsRequired(false);
        b.Property(x => x.Mobile).HasMaxLength(10).IsRequired(false);

        // Map value object to string column "Email"
        var emailConverter = new ValueConverter<EmailAddress, string>(
            v => v.Value,
            v => new EmailAddress(v));

        b.Property(x => x.Email)
            .HasConversion(emailConverter)
            .HasColumnName("Email")
            .HasMaxLength(256)
            .IsRequired(false);

        // relationships etc. as needed
        b.HasOne(x => x.Rfq)
            .WithMany(r => r.Suppliers)
            .HasForeignKey(x => x.RfqId);
    }
}