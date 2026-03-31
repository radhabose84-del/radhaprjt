using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class RepackingMasterConfiguration : IEntityTypeConfiguration<RepackingMaster>
    {
        public void Configure(EntityTypeBuilder<RepackingMaster> builder)
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

            builder.ToTable("RepackingMaster", "Production");
            builder.HasKey(t => t.Id);

            // Document
            builder.Property(t => t.RepackDocNo)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.RepackDate)
                .HasColumnType("date")
                .HasConversion(dateOnlyConverter)
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProductionYear)
                .HasColumnType("int")
                .IsRequired();

            // Item
            builder.Property(t => t.ItemId)
                .HasColumnType("int")
                .IsRequired();

            // Selection
            builder.Property(t => t.SelectionModeId)
                .HasColumnType("int");

            // Source (Old)
            builder.Property(t => t.OldPackTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldNetWeightPerPack)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.OldStartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldEndPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldTotalBags)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldNetWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.OldWarehouseId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldBinId)
                .HasColumnType("int")
                .IsRequired();

            // Target (New)
            builder.Property(t => t.PackTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NetWeightPerPack)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.StartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnType("int")
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

            // Loose
            builder.Property(t => t.LooseConeKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.LooseHandlingId)
                .HasColumnType("int");

            // Other
            builder.Property(t => t.Remarks)
                .HasColumnType("nvarchar(500)");

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
            builder.HasIndex(t => t.OldPackTypeId);
            builder.HasIndex(t => t.PackTypeId);

            // Same-module FKs
            builder.HasOne(t => t.OldPackType)
                .WithMany()
                .HasForeignKey(t => t.OldPackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.NewPackType)
                .WithMany()
                .HasForeignKey(t => t.PackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SelectionMode)
                .WithMany()
                .HasForeignKey(t => t.SelectionModeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.LooseHandling)
                .WithMany()
                .HasForeignKey(t => t.LooseHandlingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FKs (ItemId, UnitId, WarehouseId, BinId) — NO DB constraint
        }
    }
}
