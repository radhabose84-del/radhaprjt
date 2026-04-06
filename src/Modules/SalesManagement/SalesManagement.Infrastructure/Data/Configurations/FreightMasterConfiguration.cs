using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class FreightMasterConfiguration : IEntityTypeConfiguration<FreightMaster>
    {
        public void Configure(EntityTypeBuilder<FreightMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("FreightMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FreightModeId)
                .HasColumnName("FreightModeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RateMethodId)
                .HasColumnName("RateMethodId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Rate)
                .HasColumnName("Rate")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

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

            // Composite unique constraint: one rate per mode-method combination
            builder.HasIndex(t => new { t.FreightModeId, t.RateMethodId }).IsUnique();
            builder.HasIndex(t => t.FreightModeId);
            builder.HasIndex(t => t.RateMethodId);

            // Same-module FK to MiscMaster (FreightMode)
            builder.HasOne(t => t.FreightMode)
                .WithMany(m => m.FreightMastersAsFreightMode)
                .HasForeignKey(t => t.FreightModeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK to MiscMaster (RateMethod)
            builder.HasOne(t => t.RateMethod)
                .WithMany(m => m.FreightMastersAsRateMethod)
                .HasForeignKey(t => t.RateMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
