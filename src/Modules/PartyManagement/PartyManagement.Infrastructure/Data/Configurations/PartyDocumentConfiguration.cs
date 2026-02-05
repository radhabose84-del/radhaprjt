using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyDocumentConfiguration : IEntityTypeConfiguration<PartyDocument>
    {
        public void Configure(EntityTypeBuilder<PartyDocument> builder)
        {
            builder.ToTable("PartyDocument", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.PartyId)  // Foreign Key column
               .HasColumnName("PartyId")
               .HasColumnType("int")  // Set as int
               .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PartyDocumentId)
                .WithMany(t => t.PartyDocumentTypes)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.DocumentId)  // Foreign Key column
              .HasColumnName("DocumentId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.DocumentTypeMisc)
                .WithMany(t => t.PartyDocumentType)
                .HasForeignKey(m => m.DocumentId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.FileName)
               .HasColumnName("FileName")
               .HasColumnType("nvarchar(100)")
               .IsRequired();

            builder.Property(m => m.UploadedDate)
               .HasColumnName("UploadedDate")
               .HasColumnType("datetimeoffset")
               .IsRequired();    
        }
    }
}