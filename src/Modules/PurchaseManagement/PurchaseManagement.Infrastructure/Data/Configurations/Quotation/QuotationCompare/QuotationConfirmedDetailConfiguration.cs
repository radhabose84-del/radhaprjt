using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Quotation.QuotationFinal
{
    public class QuotationConfirmedDetailConfiguration : IEntityTypeConfiguration<QuotationComparisonDetail>
    {
        public void Configure(EntityTypeBuilder<QuotationComparisonDetail> builder)
        {
            builder.ToTable("QuotationComparisonDetail", "Purchase");
            // Primary Key
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.QuotationComparisonHeaderId)
                .HasColumnName("QuotationComparisonHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(m => m.QuotationComparisonHeader)
               .WithMany(g => g.QuotationConfirmedDetails)
               .HasForeignKey(m => m.QuotationComparisonHeaderId)
               .OnDelete(DeleteBehavior.Restrict);


            builder.Property(b => b.QuotationHeaderId)
                .HasColumnName("QuotationHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(m => m.QuotationHeader)
               .WithMany(g => g.ConfirmedLines)
               .HasForeignKey(m => m.QuotationHeaderId)
               .OnDelete(DeleteBehavior.Restrict);


            builder.Property(b => b.QuotationDetailId)
                .HasColumnName("QuotationDetailId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(m => m.QuotationCompareDetail)
               .WithMany(g => g.ConfirmedLinesDetails)
               .HasForeignKey(m => m.QuotationDetailId)
               .OnDelete(DeleteBehavior.Restrict);


            builder.Property(dg => dg.Net)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

            builder.Property(dg => dg.LandedUnit)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

            builder.Property(dg => dg.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
            builder.Property(t => t.OverrideStatus)
                .HasColumnName("OverrideStatus")
                .HasColumnType("bit")
                .IsRequired();

         builder.Property(s => s.Remarks)
              .IsRequired()
              .HasColumnType("nvarchar(400)");
   
        }
    }
}