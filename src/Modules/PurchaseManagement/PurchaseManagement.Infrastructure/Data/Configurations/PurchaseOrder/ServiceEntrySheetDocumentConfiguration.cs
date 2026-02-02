using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class ServiceEntrySheetDocumentConfiguration : IEntityTypeConfiguration<ServiceEntrySheetDocument>
    {
        public void Configure(EntityTypeBuilder<ServiceEntrySheetDocument> builder)
        {
           // Table name/schema
            builder.ToTable("ServiceEntrySheetDocuments", "Purchase");

            // Primary key
            builder.HasKey(d => d.Id);

            // Required properties
            builder.Property(d => d.ServiceEntrySheetId)
                   .IsRequired();

            builder.Property(d => d.DocumentId)
                   .IsRequired();

            builder.Property(d => d.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(d => d.UploadedDate)
                   .IsRequired();

            // Optional fields
            builder.Property(d => d.UploadedPath)
                   .HasMaxLength(500);

            builder.Property(d => d.DocumentName)
                   .HasMaxLength(255);

            // Relationship: ServiceEntrySheet 1 - * ServiceEntrySheetDocument
            builder.HasOne(d => d.ServiceEntrySheet)
                   .WithMany(s => s.Documents)
                   .HasForeignKey(d => d.ServiceEntrySheetId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique index example: one doc type per SES
            builder.HasIndex(d => new { d.ServiceEntrySheetId, d.DocumentId, d.FileName })
                   .IsUnique();
        }
    }
}