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

            // Document
            builder.Property(t => t.UnitId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProductionYear)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RepackDocNo)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.RepackDate)
                .HasColumnType("date")
                .HasConversion(dateOnlyConverter)
                .IsRequired();

            // Target (New)
            builder.Property(t => t.ItemId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PackTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NetWeightPerPack)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalBags)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NetWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.WarehouseId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.BinId)
                .HasColumnType("int")
                .IsRequired();

            // Source (Old) — header-level
            builder.Property(t => t.OldItemId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldPackTypeId)
                .HasColumnType("int")
                .IsRequired();

            // Loose
            builder.Property(t => t.LooseConeKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.LooseHandlingId)
                .HasColumnType("int");

            // Waste
            builder.Property(t => t.FaultId)
                .HasColumnType("int");

            builder.Property(t => t.WasteTypeId)
                .HasColumnType("int");

            builder.Property(t => t.WasteQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(t => t.WasteReason)
                .HasColumnType("nvarchar(500)");

            // Other
            builder.Property(t => t.Remarks)
                .HasColumnType("nvarchar(500)");

            builder.Property(t => t.LotId)
                .HasColumnType("int");

            // Base entity
            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedBy).HasColumnType("int");
            builder.Property(b => b.CreatedDate).HasColumnType("datetimeoffset");
            builder.Property(b => b.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(b => b.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(b => b.ModifiedBy).HasColumnType("int");
            builder.Property(b => b.ModifiedDate).HasColumnType("datetimeoffset");
            builder.Property(b => b.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(b => b.ModifiedIP).HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.RepackDocNo).IsUnique();
            builder.HasIndex(t => t.RepackDate);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.OldItemId);
            builder.HasIndex(t => t.PackTypeId);
            builder.HasIndex(t => t.OldPackTypeId);
            builder.HasIndex(t => t.WarehouseId);
            builder.HasIndex(t => t.BinId);
            builder.HasIndex(t => t.LotId);

            // Same-module FK constraints
            builder.HasOne(t => t.OldPackType)
                .WithMany()
                .HasForeignKey(t => t.OldPackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.NewPackType)
                .WithMany()
                .HasForeignKey(t => t.PackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.LooseHandling)
                .WithMany()
                .HasForeignKey(t => t.LooseHandlingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Fault)
                .WithMany()
                .HasForeignKey(t => t.FaultId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.WasteType)
                .WithMany()
                .HasForeignKey(t => t.WasteTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Lot)
                .WithMany()
                .HasForeignKey(t => t.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Header → Detail relationship
            builder.HasMany(t => t.RepackingDetails)
                .WithOne(d => d.RepackingHeader)
                .HasForeignKey(d => d.RepackHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cross-module FKs (UnitId, ItemId, OldItemId, WarehouseId, BinId) — NO DB constraint
        }
    }
}
