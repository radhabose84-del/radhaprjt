using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.GRN.GateEntry
{
    public class GateEntryDetailConfiguration : IEntityTypeConfiguration<GateEntryDetail>
    {

        public void Configure(EntityTypeBuilder<GateEntryDetail> builder)
        {
            builder.ToTable("GateEntryDetail", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.GateEntryHeaderId)
                .HasColumnName("GateEntryHeaderId")
                .HasColumnType("int")
                .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GateEntryHeaderDetails)
                .WithMany(t => t.GateEntryDetails)
                .HasForeignKey(m => m.GateEntryHeaderId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.PoCategoryId)
                   .HasColumnName("PoCategoryId")
                   .HasColumnType("int")
                   .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PoType)
                .WithMany(t => t.PoTypeGateEntry)
                .HasForeignKey(m => m.PoCategoryId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.PoId)
                .HasColumnName("PoId")
                .HasColumnType("int")
                .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GatePurchaseOrder)
                .WithMany(t => t.POGateEntriesDetails)
                .HasForeignKey(m => m.PoId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed


            builder.Property(b => b.PoDate)
                   .HasColumnName("PoDate")
                   .IsRequired()
                   .HasColumnType("DatetimeOffset");

            builder.Property(m => m.PoCreatedBy)
                   .HasColumnName("PoCreatedBy")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(m => m.GSTNumber)
               .HasColumnName("GSTNumber")
               .HasColumnType("nvarchar(100)");


            builder.Property(m => m.ContactDetails)
                   .HasColumnName("ContactDetails")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();
                   
            builder.Property(m => m.PoMethodId)
                   .HasColumnName("PoMethodId")
                   .HasColumnType("int")
                   .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PoGateMethodDetails)
                .WithMany(t => t.GateEntryDetailsPoMethod)
                .HasForeignKey(m => m.PoMethodId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

                    
        }
    }
}