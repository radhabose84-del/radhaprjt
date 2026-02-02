using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class TnCTemplateMasterConfiguration : IEntityTypeConfiguration<TnCTemplateMaster>
    {
        public void Configure(EntityTypeBuilder<TnCTemplateMaster> builder)
        {

            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            builder.ToTable("TnCTemplateMaster", "Purchase");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateCode)
            .HasColumnType("nvarchar(50)")
            .IsRequired();

            builder.Property(x => x.TemplateName)
            .HasColumnType("nvarchar(200)")
            .IsRequired();

            builder.Property(x => x.TemplateTypeId)
            .HasColumnType("int")
            .IsRequired();

            builder.HasOne(x => x.TemplateType)
            .WithMany(m => m.TncTemplatesByType)
            .HasForeignKey(x => x.TemplateTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.TermsHtml)
             .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Optional ApprovalFlag
            builder.Property(x => x.ApprovalFlag)
               .HasColumnType("bit");
              

            builder.HasMany(x => x.Applicabilities)
                .WithOne(x => x.TnCTemplate)
                .HasForeignKey(x => x.TnCTemplateMasterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TemplateTypeId, x.TemplateName })
                .IsUnique()
                .HasDatabaseName("UX_TnC_Type_Name")
                .HasFilter("[IsDeleted] = 0");  // Filter deleted records  

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

         builder.Property(b => b.IsDeleted)
                  .HasColumnName("IsDeleted")
                  .HasColumnType("bit")
                  .HasConversion(isDeleteConverter)
                  .IsRequired();

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");   
        }
    }
}