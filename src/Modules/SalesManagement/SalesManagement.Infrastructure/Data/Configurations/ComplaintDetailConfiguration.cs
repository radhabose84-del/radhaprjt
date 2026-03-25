using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintDetailConfiguration : IEntityTypeConfiguration<ComplaintDetail>
    {
        public void Configure(EntityTypeBuilder<ComplaintDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v)
            );

            builder.ToTable("ComplaintDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintHeaderId).HasColumnName("ComplaintHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceHeaderId).HasColumnName("InvoiceHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceDate).HasColumnName("InvoiceDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();
            builder.Property(t => t.InvoiceTypeId).HasColumnName("InvoiceTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnName("LotId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.NumberOfPacks).HasColumnName("NumberOfPacks").HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeight).HasColumnName("NetWeight").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.InvoiceAmount).HasColumnName("InvoiceAmount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.DivisionId).HasColumnName("DivisionId").HasColumnType("int").IsRequired(false);

            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK → InvoiceHeader
            builder.HasOne(t => t.InvoiceHeader)
                .WithMany()
                .HasForeignKey(t => t.InvoiceHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (InvoiceType)
            builder.HasOne(t => t.InvoiceTypeMisc)
                .WithMany()
                .HasForeignKey(t => t.InvoiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection → ComplaintDetailNature
            builder.HasMany(t => t.ComplaintDetailNatures)
                .WithOne(n => n.ComplaintDetail)
                .HasForeignKey(n => n.ComplaintDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.ComplaintHeaderId);
            builder.HasIndex(t => t.InvoiceHeaderId);
            builder.HasIndex(t => t.ItemId);
        }
    }
}
