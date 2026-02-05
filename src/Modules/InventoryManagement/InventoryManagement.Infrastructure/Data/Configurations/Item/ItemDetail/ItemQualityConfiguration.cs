using InventoryManagement.Domain.Entities.Item.ItemDetail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail
{
    public sealed class ItemQualityConfiguration : IEntityTypeConfiguration<ItemQuality>
    {
        public void Configure(EntityTypeBuilder<ItemQuality> b)
        {
            b.ToTable("ItemQuality", "Inventory");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
             .HasColumnName("Id")
             .HasColumnType("int")
             .UseIdentityColumn();   

            b.Property(x => x.ItemId)
             .HasColumnName("ItemId")
             .HasColumnType("int")
             .IsRequired();            
            b.HasOne(x => x.Item)
             .WithOne(i => i.Quality)
             .HasForeignKey<ItemQuality>(x => x.ItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.InspectionTemplateId)
             .HasColumnName("InspectionTemplateId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.InspectionTemplate)
             .WithMany(t => t.Items)
             .HasForeignKey(x => x.InspectionTemplateId)
             .OnDelete(DeleteBehavior.Restrict); 

            b.Property(x => x.CertificateTypeId)
             .HasColumnName("CertificateTypeId")
             .HasColumnType("int")
             .IsRequired(false);
            b.HasOne(x => x.MiscCertificateType)
            .WithMany(i => i.ItemQualityCertificateType)             
             .HasForeignKey(x => x.CertificateTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.InspLotProcessingTime)
             .HasColumnName("InspLotProcessingTime")
             .HasColumnType("int")
             .IsRequired(false);

            b.Property(x => x.InspectionRequired)
             .HasColumnName("InspectionRequired")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.QualityInspectionFree)
             .HasColumnName("QualityInspectionFree")
             .HasColumnType("bit")
             .IsRequired();

            b.Property(x => x.IsCertificateRequiredFromSupplier)
             .HasColumnName("IsCertificateRequiredFromSupplier")
             .HasColumnType("bit")
             .IsRequired();
          
        }
    }
}
