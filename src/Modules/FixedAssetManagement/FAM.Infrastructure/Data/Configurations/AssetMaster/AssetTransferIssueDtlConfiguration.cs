using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetTransferIssueDtlConfiguration : IEntityTypeConfiguration<AssetTransferIssueDtl>
    {
        public void Configure(EntityTypeBuilder<AssetTransferIssueDtl> builder)
        {
        builder.ToTable("AssetTransferIssueDtl", "FixedAsset");

              // Primary Key
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

        builder.Property(dg => dg.AssetTransferId)                
                .HasColumnType("int")
                .IsRequired();

        // Configure Foreign Key Relationship: AssetTransferId (One-to-Many)
        builder.HasOne(dtl => dtl.AssetTransferIssueHdr)
            .WithMany(hdr => hdr.AssetTransferIssueDtl) // One AssetTransferIssueHdr has many AssetTransferIssueDtl
            .HasForeignKey(dtl => dtl.AssetTransferId)  
            .OnDelete(DeleteBehavior.Cascade); // Delete details if header is deleted

        builder.Property(dg => dg.AssetId)                
                .HasColumnType("int")
                .IsRequired();
            
        // One-to-one relationship with AssetMasterGenerals
        builder.HasOne(x => x.AssetMasterTransferIssue)
             .WithMany(hdr => hdr.AssetTransferIssueMaster) // One AssetTransferIssueDtl has many AssetId
            .HasForeignKey(dtl => dtl.AssetId) 
            .OnDelete(DeleteBehavior.Cascade); // Delete details if header is deleted

        builder.Property(dg => dg.AssetValue)                
                .HasColumnType("decimal(18,3)")                
                .IsRequired();
        
        }
    }
}