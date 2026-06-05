using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class BarcodeSeriesConfiguration : IEntityTypeConfiguration<BarcodeSeries>
    {
        public void Configure(EntityTypeBuilder<BarcodeSeries> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("BarcodeSeries", "Purchase");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.BarcodeSeriesNumber)
                .HasColumnName("BarcodeSeriesNumber")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(b => b.PrefixId)
                .HasColumnName("PrefixId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.BarcodeStartNumber)
                .HasColumnName("BarcodeStartNumber")
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(b => b.BarcodeEndNumber)
                .HasColumnName("BarcodeEndNumber")
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(b => b.GenerationDate)
                .HasColumnName("GenerationDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.AllocatedCount)
                .HasColumnName("AllocatedCount")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(b => b.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(250)");

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

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");

            // Indexes
            builder.HasIndex(b => b.BarcodeSeriesNumber).IsUnique();
            builder.HasIndex(b => b.PrefixId);
            builder.HasIndex(b => b.StatusId);

            // Same-module FK -> Purchase.MiscMaster (Prefix)
            builder.HasOne(b => b.Prefix)
                .WithMany(m => m.BarcodeSeriesPrefixes)
                .HasForeignKey(b => b.PrefixId)
                .HasPrincipalKey(m => m.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> Purchase.MiscMaster (Status)
            builder.HasOne(b => b.MiscStatus)
                .WithMany(m => m.BarcodeSeriesStatuses)
                .HasForeignKey(b => b.StatusId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
