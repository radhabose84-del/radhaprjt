using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetTransferIssueHdrConfiguration : IEntityTypeConfiguration<AssetTransferIssueHdr>
    {
        public void Configure(EntityTypeBuilder<AssetTransferIssueHdr> builder)
        {
            builder.ToTable("AssetTransferIssueHdr", "FixedAsset");
                // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(dg => dg.DocDate)   
                .HasColumnType("datetimeoffset")                             
                .IsRequired();

            builder.Property(dg => dg.TransferType)
                .HasColumnType("int")
                .IsRequired(); 

             // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.TransferTypeIssueMiscType)
                .WithMany(ag => ag.AssetTransferIssueType)
                .HasForeignKey(dg => dg.TransferType)                
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(dg => dg.FromUnitId)
                .HasColumnType("int")
                .IsRequired(); 

            builder.Property(dg => dg.ToUnitId)
                .HasColumnType("int")
                .IsRequired(); 
            
            builder.Property(dg => dg.FromDepartmentId)
                .HasColumnType("int")
                .IsRequired(); 

            builder.Property(dg => dg.ToDepartmentId)
                .HasColumnType("int")
                .IsRequired(); 

            builder.Property(dg => dg.FromCustodianId)
                .HasColumnType("int")
                .IsRequired(); 

            builder.Property(b => b.FromCustodianName)
                .IsRequired()
                .HasColumnType("nvarchar(100)");    

            builder.Property(dg => dg.ToCustodianId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.ToCustodianName)
                .IsRequired()
                .HasColumnType("nvarchar(100)");

            builder.Property(b => b.GatePassNo)
                .HasColumnType("nvarchar(100)");   

            builder.Property(b => b.Status)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(b => b.AckStatus)                              
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();  

            builder.Property(b => b.CreatedBy)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(dg => dg.CreatedDate)   
                .HasColumnType("datetimeoffset")                             
                .IsRequired(); 
    
            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedBy)
                .HasColumnType("varchar(50)")
                 .IsRequired(false); 

            builder.Property(dg => dg.ModifiedDate)   
                .HasColumnType("datetimeoffset")                             
                .IsRequired(false); 
    
            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)")
                 .IsRequired(false); 

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(50)")
                .IsRequired(false); 


            builder.Property(b => b.AuthorizedBy)
                .HasColumnType("varchar(50)")
                .IsRequired(false); 

            builder.Property(dg => dg.AuthorizedDate)   
                .HasColumnType("datetimeoffset")                             
                .IsRequired(false); 
    
            builder.Property(b => b.AuthorizedByName)
                .HasColumnType("varchar(50)")
                 .IsRequired(false); 

            builder.Property(b => b.AuthorizedIP)
                .HasColumnType("varchar(50)")
                .IsRequired(false); 

            builder.HasMany(x => x.AssetTransferIssueDtl)
            .WithOne(x => x.AssetTransferIssueHdr)
            .HasForeignKey(x => x.AssetTransferId)
            .OnDelete(DeleteBehavior.Cascade); // Ensure deletion cascades if needed

            

            





            

            

        }
    }
}