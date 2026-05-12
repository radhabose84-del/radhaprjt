using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesAgreementDetailConfiguration : IEntityTypeConfiguration<SalesAgreementDetail>
    {
        public void Configure(EntityTypeBuilder<SalesAgreementDetail> builder)
        {
            builder.ToTable("SalesAgreementDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesAgreementHeaderId)
                .HasColumnName("SalesAgreementHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(t => t.SalesAgreementHeader)
                .WithMany(h => h.SalesAgreementDetails)
                .HasForeignKey(t => t.SalesAgreementHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.VariantId)
                .HasColumnName("VariantId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.UomId)
                .HasColumnName("UomId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.AgreedRate)
                .HasColumnName("AgreedRate")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.TotalQty)
                .HasColumnName("TotalQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ReleasedQty)
                .HasColumnName("ReleasedQty")
                .HasColumnType("decimal(18,3)")
                .HasDefaultValue(0m)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.SalesAgreementHeaderId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.VariantId);
            builder.HasIndex(t => t.UomId);

            // Composite unique: one row per (Header, Item, Variant). SQL Server treats NULL as a single value,
            // so a no-variant item can still have only one row per agreement.
            builder.HasIndex(t => new { t.SalesAgreementHeaderId, t.ItemId, t.VariantId })
                .IsUnique();
        }
    }
}
