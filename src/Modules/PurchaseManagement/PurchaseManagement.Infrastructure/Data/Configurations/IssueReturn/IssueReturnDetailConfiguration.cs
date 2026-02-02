using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.IssueReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.IssueReturn
{
    public class IssueReturnDetailConfiguration  : IEntityTypeConfiguration<IssueReturnDetail>
    {
        public void Configure(EntityTypeBuilder<IssueReturnDetail> builder)
        {
            builder.ToTable("IssueReturnDetail", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();


            builder.Property(m => m.IssueReturnHeaderId)
                 .HasColumnName("IssueReturnHeaderId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.IssueReturnHeaderDetails)
                .WithMany(t => t.IssueReturnDetailsHeaderName)
                .HasForeignKey(m => m.IssueReturnHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //

            builder.Property(m => m.ItemId)
                 .HasColumnName("ItemId")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(m => m.UomId)
                 .HasColumnName("UomId")
                 .HasColumnType("int")
                 .IsRequired();


            builder.Property(m => m.WarehouseStockId)
               .HasColumnName("WarehouseStockId")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(m => m.StorageTypeId)
            .HasColumnName("StorageTypeId")
            .HasColumnType("int")
            .IsRequired(false);


            builder.Property(m => m.TargetId)
           .HasColumnName("TargetId")
           .HasColumnType("int")
           .IsRequired(false);

            builder.Property(dg => dg.ReturnQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(dg => dg.ReturnValue)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(m => m.ReasonId)
               .HasColumnName("ReasonId")
               .HasColumnType("int")
               .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.IssueDetailReason)
                .WithMany(t => t.IssueReturnDetailsReasonMisc)
                .HasForeignKey(m => m.ReasonId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);


            builder.Property(b => b.Remarks)
                    .HasColumnType("nvarchar(250)");


            builder.Property(m => m.CreatedBy)
               .HasColumnName("CreatedBy")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(b => b.CreatedDate)
                    .HasColumnName("CreatedDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");


            builder.Property(m => m.ApprovedBy)
                .HasColumnName("ApprovedBy");


            builder.Property(b => b.ApprovedDate)
                    .HasColumnName("ApprovedDate")
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.ApprovedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ApprovedIP)
                    .HasColumnType("varchar(20)");


            builder.Property(m => m.StatusId)
                 .HasColumnName("StatusId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.StatusIssueDetailHeader)
                .WithMany(t => t.IssueReturnDetailMiscRequestHeader)
                .HasForeignKey(m => m.StatusId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);
                
                  
            builder.Property(m => m.SubStoresDepartmentId)
                 .HasColumnName("SubStoresDepartmentId")
                 .HasColumnType("int")
                 .IsRequired(false);

            
        }
    }
}