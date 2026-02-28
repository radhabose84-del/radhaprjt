using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class CustomerVisitConfiguration : IEntityTypeConfiguration<CustomerVisit>
    {
        public void Configure(EntityTypeBuilder<CustomerVisit> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CustomerVisit", "Sales");
            builder.HasKey(t => t.Id);

            // Customer (cross-module FK — no DB constraint)
            builder.Property(t => t.CustomerId)
                .HasColumnType("int")
                .IsRequired();

            // Visit details
            builder.Property(t => t.VisitTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.VisitDateTime)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            // Geo-location
            builder.Property(t => t.Latitude)
                .HasColumnType("decimal(10,7)")
                .IsRequired(false);

            builder.Property(t => t.Longitude)
                .HasColumnType("decimal(10,7)")
                .IsRequired(false);

            // Photo
            builder.Property(t => t.ImageName)
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            // Notes
            builder.Property(t => t.Remarks)
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            // Ownership
            builder.Property(t => t.MarketingOfficerId)
                .HasColumnType("int")
                .IsRequired();

            // Status
            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.CustomerId)
                .HasDatabaseName("IX_CustomerVisit_CustomerId");

            builder.HasIndex(t => t.VisitTypeId)
                .HasDatabaseName("IX_CustomerVisit_VisitTypeId");

            builder.HasIndex(t => t.MarketingOfficerId)
                .HasDatabaseName("IX_CustomerVisit_MarketingOfficerId");

            builder.HasIndex(t => t.VisitDateTime)
                .HasDatabaseName("IX_CustomerVisit_VisitDateTime");

            // Same-module FK: VisitTypeId → Sales.MiscMaster
            builder.HasOne(t => t.VisitType)
                .WithMany()
                .HasForeignKey(t => t.VisitTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: MarketingOfficerId → Sales.MarketingOfficer
            builder.HasOne(t => t.MarketingOfficer)
                .WithMany()
                .HasForeignKey(t => t.MarketingOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FK: CustomerId → PartyManagement — no DB constraint
        }
    }
}
