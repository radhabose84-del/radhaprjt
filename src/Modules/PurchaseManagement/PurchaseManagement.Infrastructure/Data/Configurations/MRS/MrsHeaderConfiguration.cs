using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.MRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.MRS
{
    public class MrsHeaderConfiguration : IEntityTypeConfiguration<MrsHeader>
    {
        public void Configure(EntityTypeBuilder<MrsHeader> builder)
        {
            builder.ToTable("MrsHeader", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.UnitId)
                   .HasColumnName("UnitId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.RequestCategoryId)
                 .HasColumnName("RequestCategoryId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.StatusRequest)
                .WithMany(t => t.MrsRequestHeader)
                .HasForeignKey(m => m.RequestCategoryId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.MrsNo)
                   .HasColumnName("MrsNo")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(b => b.MrsDate)
                    .HasColumnName("MrsDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(m => m.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.SubDepartmentId)
               .HasColumnName("SubDepartmentId")
               .HasColumnType("int")
               .IsRequired();


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

            builder.Property(m => m.ModifiedBy)
                 .HasColumnName("ModifiedBy")
                 .HasColumnType("int");

            builder.Property(b => b.ModifiedDate)
                    .HasColumnName("ModifiedDate")
                    .IsRequired(false)
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");


            builder.Property(m => m.ApprovedBy)
                .HasColumnName("ApprovedBy")
                .HasColumnType("int");

            builder.Property(b => b.ApprovedDate)
                    .HasColumnName("ApprovedDate")
                    .IsRequired(false)
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.ApprovedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ApprovedIP)
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.Remarks)
                    .HasColumnType("nvarchar(250)");

            builder.Property(m => m.StatusId)
                 .HasColumnName("StatusId")
                 .HasColumnType("int")
                 .IsRequired();
                 
               builder.Property(m => m.SubStoresWarehouseId)
                 .HasColumnName("SubStoresWarehouseId")
                 .HasColumnType("int")
                 .IsRequired(false);

            // Foreign Key Relationship
                        builder.HasOne(m => m.StatusMrs)
                .WithMany(t => t.MrsDetailsHeader)
                .HasForeignKey(m => m.StatusId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}