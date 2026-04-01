using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderAmendmentHeaderConfiguration : IEntityTypeConfiguration<SalesOrderAmendmentHeader>
    {
        public void Configure(EntityTypeBuilder<SalesOrderAmendmentHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesOrderAmendmentHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderHeaderId)
                .HasColumnName("SalesOrderHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AmendmentNo)
                .HasColumnName("AmendmentNo")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.RevisionNumber)
                .HasColumnName("RevisionNumber")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AmendmentDate)
                .HasColumnName("AmendmentDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.Reason)
                .HasColumnName("Reason")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            // Header-level Summary Fields
            builder.Property(t => t.TotalBags).HasColumnName("TotalBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.TotalWeightKgs).HasColumnName("TotalWeightKgs").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalDiscountPerKg).HasColumnName("TotalDiscountPerKg").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ItemValue).HasColumnName("ItemValue").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalFreight).HasColumnName("TotalFreight").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TaxableAmount).HasColumnName("TaxableAmount").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.GSTPercentage).HasColumnName("GSTPercentage").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalGST).HasColumnName("TotalGST").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalWithGST).HasColumnName("TotalWithGST").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TCSPercentage).HasColumnName("TCSPercentage").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalTCS).HasColumnName("TotalTCS").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.FinalAmount).HasColumnName("FinalAmount").HasColumnType("decimal(18,3)").IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ApprovedBy)
                .HasColumnName("ApprovedBy")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ApprovedDate)
                .HasColumnName("ApprovedDate")
                .IsRequired(false);

            // Status & Audit
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

            // Same-module FK constraints
            builder.HasOne(t => t.SalesOrderHeader)
                .WithMany(h => h.SalesOrderAmendmentHeaders)
                .HasForeignKey(t => t.SalesOrderHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesOrderAmendmentHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection — reverse navigation (Header → Details)
            builder.HasMany(t => t.SalesOrderAmendmentDetails)
                .WithOne(d => d.SalesOrderAmendmentHeader)
                .HasForeignKey(d => d.SalesOrderAmendmentHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.AmendmentNo).IsUnique();
            builder.HasIndex(t => t.SalesOrderHeaderId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.StatusId);
        }
    }
}
