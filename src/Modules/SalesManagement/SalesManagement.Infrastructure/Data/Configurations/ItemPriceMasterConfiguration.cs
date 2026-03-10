using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ItemPriceMasterConfiguration : IEntityTypeConfiguration<ItemPriceMaster>
    {
        public void Configure(EntityTypeBuilder<ItemPriceMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ItemPriceMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PriceCode)
                .HasColumnName("PriceCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesSegmentId)
                .HasColumnName("SalesSegmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PaymentTermsId)
                .HasColumnName("PaymentTermsId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ExMillRate)
                .HasColumnName("ExMillRate")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.CurrencyId)
                .HasColumnName("CurrencyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ValidFrom)
                .HasColumnName("ValidFrom")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ValidTo)
                .HasColumnName("ValidTo")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Unique index on PriceCode
            builder.HasIndex(t => t.PriceCode).IsUnique();

            // Composite index for overlap query performance
            builder.HasIndex(t => new { t.ItemId, t.SalesSegmentId, t.PaymentTermsId });

            // Same-module FK — SalesSegment (DB constraint created)
            builder.HasOne(t => t.SalesSegment)
                .WithMany()
                .HasForeignKey(t => t.SalesSegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK — StatusId → MiscMaster
            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.ItemPriceMastersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FKs (ItemId, PaymentTermsId, CurrencyId) — NO DB FK constraints
        }
    }
}
