using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class RepackingHeaderConfiguration : IEntityTypeConfiguration<RepackingHeader>
    {
        public void Configure(EntityTypeBuilder<RepackingHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

            builder.ToTable("RepackingHeader", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UnitId).HasColumnType("int").IsRequired();
            builder.Property(t => t.ProductionYear).HasColumnType("int").IsRequired();
            builder.Property(t => t.RepackingNo).HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.RepackingDate)
                .HasColumnType("date")
                .HasConversion(dateOnlyConverter)
                .IsRequired();
            builder.Property(t => t.TotalBags).HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeight).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.LooseConeKgs).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.OldPackHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.LooseHandlingId).HasColumnType("int");
            builder.Property(t => t.Remarks).HasColumnType("nvarchar(500)");

            builder.Property(t => t.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();
            builder.Property(t => t.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int").IsRequired();
            builder.Property(t => t.CreatedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            builder.HasIndex(t => t.RepackingNo).IsUnique();
            builder.HasIndex(t => t.OldPackHeaderId);
            builder.HasIndex(t => t.RepackingDate);

            // Same-module FK: OldPackHeaderId → ProductionPackHeader
            builder.HasOne(t => t.OldPackHeader)
                .WithMany()
                .HasForeignKey(t => t.OldPackHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: LooseHandlingId → MiscMaster (optional)
            builder.HasOne(t => t.LooseHandling)
                .WithMany()
                .HasForeignKey(t => t.LooseHandlingId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many with RepackingDetail
            builder.HasMany(t => t.RepackingDetails)
                .WithOne(d => d.RepackingHeader)
                .HasForeignKey(d => d.RepackingHeaderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
