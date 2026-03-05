using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StoDetailConfiguration : IEntityTypeConfiguration<StoDetail>
    {
        public void Configure(EntityTypeBuilder<StoDetail> builder)
        {
            builder.ToTable("StoDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoHeaderId)
                .HasColumnName("StoHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Quantity)
                .HasColumnName("Quantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.UOMId)
                .HasColumnName("UOMId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TransferPrice)
                .HasColumnName("TransferPrice")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.LineStatusId)
                .HasColumnName("LineStatusId")
                .HasColumnType("int")
                .IsRequired(false);

            // Same-module FK: StoDetail → MiscMaster (LineStatus)
            builder.HasOne(t => t.LineStatus)
                .WithMany(m => m.StoDetailsAsLineStatus)
                .HasForeignKey(t => t.LineStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.StoHeaderId);
            builder.HasIndex(t => t.ItemId);
        }
    }
}
