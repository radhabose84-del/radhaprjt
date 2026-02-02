using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Quotation.QuotationFinal
{
    public class QuotationConfirmedHeaderConfiguration : IEntityTypeConfiguration<QuotationComparisonHeader>
    {
        public void Configure(EntityTypeBuilder<QuotationComparisonHeader> builder)
        {
            builder.ToTable("QuotationComparisonHeader", "Purchase");
            // Primary Key
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(s => s.RfqId)
               .IsRequired()
               .HasColumnType("int");

            builder.HasOne(m => m.Rfq)
               .WithMany(g => g.QuotationrfqConfirmed)
               .HasForeignKey(m => m.RfqId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(q => q.RfqCode)
                .IsRequired()
                .HasColumnType("varchar(30)");

            builder.Property(dg => dg.ConfirmedDate)
              .HasColumnName("ConfirmedDate")
              .HasColumnType("datetimeoffset")
              .IsRequired();

            builder.Property(b => b.CreatedBy)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(dg => dg.CreatedDate)
             .HasColumnName("CreatedDate")
             .HasColumnType("datetimeoffset")
             .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(m => m.StatusId)
              .HasColumnName("StatusId")
              .HasColumnType("int")
              .IsRequired();

            builder.HasOne(ac => ac.StatusQuotation)
             .WithMany(am => am.StatusWorkflow)
             .HasForeignKey(ac => ac.StatusId)
             .OnDelete(DeleteBehavior.NoAction);


             builder.Property(b => b.ModifiedBy)
                .IsRequired(false)
                .HasColumnType("int");

            builder.Property(dg => dg.ModifiedDate)
             .HasColumnName("ModifiedDate")
             .HasColumnType("datetimeoffset")
             .IsRequired(false);

            builder.Property(b => b.ModifiedByName)
                .IsRequired(false)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .IsRequired(false)
                .HasColumnType("varchar(50)");
                
                             
        }
    }
}