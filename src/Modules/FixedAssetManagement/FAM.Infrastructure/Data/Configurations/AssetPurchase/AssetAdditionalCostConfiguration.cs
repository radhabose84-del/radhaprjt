using FAM.Domain.Entities.AssetPurchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetPurchase
{
    public class AssetAdditionalCostConfiguration: IEntityTypeConfiguration<AssetAdditionalCost>
    {
        public void Configure(EntityTypeBuilder<AssetAdditionalCost> builder)
        {
             builder.ToTable("AssetAdditionalCost", "FixedAsset");
              
              // Primary Key
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(ac => ac.AssetId)
                .HasColumnName("AssetId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(ac => ac.AssetSourceId)
                .HasColumnName("AssetSourceId")
                .HasColumnType("int")
                .IsRequired();

                 builder.Property(ac => ac.CostType)
                .HasColumnName("CostType")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(b => b.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

                builder.Property(b => b.JournalNo)
                .HasColumnName("JournalNo")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

                  // Relationships
            builder.HasOne(b => b.Asset)
                .WithMany(pu => pu.AssetAdditionalCost)
                .HasForeignKey(b => b.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.AssetSource)
                .WithMany(pu => pu.AssetAdditionalCost)
                .HasForeignKey(b => b.AssetSourceId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(b => b.CostMiscType)
                .WithMany(pu => pu.AssetAdditionalCost)
                .HasForeignKey(b => b.CostType)
                .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}