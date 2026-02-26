using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DispatchAddressMappingConfiguration : IEntityTypeConfiguration<DispatchAddressMapping>
    {
        public void Configure(EntityTypeBuilder<DispatchAddressMapping> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DispatchAddressMapping", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.PartyId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchAddressId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UsageTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.IsDefault)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Composite unique index: one record per Party + DispatchAddress + UsageType (non-deleted)
            builder.HasIndex(t => new { t.PartyId, t.DispatchAddressId, t.UsageTypeId })
                .HasDatabaseName("UIX_DispatchAddressMapping_Composite")
                .HasFilter("[IsDeleted] = 0");

            // Performance indexes
            builder.HasIndex(t => t.PartyId)
                .HasDatabaseName("IX_DispatchAddressMapping_PartyId");

            builder.HasIndex(t => t.DispatchAddressId)
                .HasDatabaseName("IX_DispatchAddressMapping_DispatchAddressId");

            builder.HasIndex(t => t.UsageTypeId)
                .HasDatabaseName("IX_DispatchAddressMapping_UsageTypeId");

            // Same-module FK: DispatchAddressId → Sales.DispatchAddressMaster
            builder.HasOne(t => t.DispatchAddress)
                .WithMany(d => d.DispatchAddressMappings)
                .HasForeignKey(t => t.DispatchAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: UsageTypeId → Sales.MiscMaster
            builder.HasOne(t => t.UsageType)
                .WithMany(m => m.DispatchAddressMappings)
                .HasForeignKey(t => t.UsageTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // No FK constraint for PartyId (cross-module)
        }
    }
}
