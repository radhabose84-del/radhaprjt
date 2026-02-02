using PurchaseManagement.Domain.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class PurchaseDocumentConfiguration : IEntityTypeConfiguration<PurchaseDocument>
    {
        public void Configure(EntityTypeBuilder<PurchaseDocument> builder)
        {
            builder.ToTable("PurchaseDocuments", "Purchase");

            builder.HasKey(pd => pd.Id);

            builder.Property(pd => pd.PoId).IsRequired();
            builder.Property(pd => pd.DocumentId).IsRequired();
            builder.Property(pd => pd.FileName).IsRequired().HasMaxLength(255);
            builder.Property(pd => pd.UploadedDate).IsRequired();

            // Foreign key relationship
            builder.HasOne(pd => pd.MiscMaster)
                   .WithMany()
                   .HasForeignKey(pd => pd.DocumentId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.PODocumentId)
                .WithMany(t => t.PurchaseDocumentTypes)
                .HasForeignKey(m => m.PoId)
                .OnDelete(DeleteBehavior.Restrict); 
                
            builder.HasIndex(x => new { x.PoId, x.DocumentId })
                .IsUnique(); 
        }
    }
}
