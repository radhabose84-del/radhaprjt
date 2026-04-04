using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesReturnDetailConfiguration : IEntityTypeConfiguration<SalesReturnDetail>
    {
        public void Configure(EntityTypeBuilder<SalesReturnDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesReturnDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.SalesReturnHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceDetailId).HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.StartPackNo).HasColumnType("int").IsRequired();
            builder.Property(t => t.EndPackNo).HasColumnType("int").IsRequired();
            builder.Property(t => t.ReturnQty).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.PackTypeId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.BagStatusId).HasColumnType("int").IsRequired();

            builder.Property(b => b.IsActive).HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.SalesReturnHeaderId);
            builder.HasIndex(t => t.InvoiceHeaderId);
            builder.HasIndex(t => t.InvoiceDetailId);

            // Same-module FKs
            builder.HasOne(t => t.InvoiceHeader)
                .WithMany()
                .HasForeignKey(t => t.InvoiceHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.BagStatus)
                .WithMany()
                .HasForeignKey(t => t.BagStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
