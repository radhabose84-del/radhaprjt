using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesReturnHeaderConfiguration : IEntityTypeConfiguration<SalesReturnHeader>
    {
        public void Configure(EntityTypeBuilder<SalesReturnHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesReturnHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ReturnNumber).HasColumnType("varchar(30)").IsRequired();
            builder.Property(t => t.ReturnDate).HasColumnType("date").IsRequired();
            builder.Property(t => t.ComplaintHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.CustomerId).HasColumnType("int").IsRequired();
            builder.Property(t => t.WarehouseId).HasColumnType("int").IsRequired();
            builder.Property(t => t.BinId).HasColumnType("int").IsRequired();
            builder.Property(t => t.StatusId).HasColumnType("int").IsRequired();
            builder.Property(t => t.Remarks).HasColumnType("nvarchar(500)").IsRequired(false);

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
            builder.HasIndex(t => t.ReturnNumber).IsUnique();
            builder.HasIndex(t => t.ComplaintHeaderId);
            builder.HasIndex(t => t.CustomerId);

            // Same-module FKs
            builder.HasOne(t => t.ComplaintHeader)
                .WithMany()
                .HasForeignKey(t => t.ComplaintHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Status)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection
            builder.HasMany(t => t.SalesReturnDetails)
                .WithOne(d => d.SalesReturnHeader)
                .HasForeignKey(d => d.SalesReturnHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
