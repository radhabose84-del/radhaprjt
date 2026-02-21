#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesSegmentConfiguration : IEntityTypeConfiguration<SalesSegment>
    {
        public void Configure(EntityTypeBuilder<SalesSegment> builder)
        {
            // Value converters for enums
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            // Table mapping
            builder.ToTable("SalesSegment", "Sales");
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.SalesOrganisationId)
                .IsRequired();

            builder.Property(s => s.SalesChannelId)
                .IsRequired();

            builder.Property(s => s.BusinessUnitId)
                .IsRequired();

            builder.Property(s => s.CurrencyId)
                .IsRequired(false);

            builder.Property(s => s.SegmentName)
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.Property(s => s.ValidFrom)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(s => s.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.SalesOrganisationId);
            builder.HasIndex(s => s.SalesChannelId);
            builder.HasIndex(s => s.BusinessUnitId);
            builder.HasIndex(s => s.CurrencyId);

            // Unique composite key (only for non-deleted records)
            builder.HasIndex(s => new { s.SalesOrganisationId, s.SalesChannelId, s.BusinessUnitId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Foreign Key Relationships with Navigation Properties (Same-Module)
            builder.HasOne(s => s.SalesOrganisation)
                .WithMany(o => o.SalesSegments)
                .HasForeignKey(s => s.SalesOrganisationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.SalesChannel)
                .WithMany(c => c.SalesSegments)
                .HasForeignKey(s => s.SalesChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.BusinessUnit)
                .WithMany(b => b.SalesSegments)
                .HasForeignKey(s => s.BusinessUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: CurrencyId - Cross-module FK: NO navigation property, NO FK constraint
            // Validated in code via ICurrencyLookup
        }
    }
}
