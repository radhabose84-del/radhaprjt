using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesEnquiryDetailConfiguration : IEntityTypeConfiguration<SalesEnquiryDetail>
    {
        public void Configure(EntityTypeBuilder<SalesEnquiryDetail> builder)
        {
            builder.ToTable("SalesEnquiryDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesEnquiryHeaderId)
                .HasColumnName("SalesEnquiryHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(t => t.SalesEnquiryHeader)
                .WithMany(h => h.SalesEnquiryDetails)
                .HasForeignKey(t => t.SalesEnquiryHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Quantity)
                .HasColumnName("Quantity")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.ExmillRate)
                .HasColumnName("ExmillRate")
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            builder.Property(t => t.TargetPrice)
                .HasColumnName("TargetPrice")
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            builder.Property(t => t.Discount)
                .HasColumnName("Discount")
                .HasColumnType("decimal(18,6)")
                .IsRequired(false);

            builder.HasIndex(t => t.SalesEnquiryHeaderId);
            builder.HasIndex(t => t.ItemId);
        }
    }
}
