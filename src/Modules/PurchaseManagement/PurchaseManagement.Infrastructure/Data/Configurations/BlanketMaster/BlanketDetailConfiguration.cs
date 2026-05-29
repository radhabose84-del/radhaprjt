using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster;

public class BlanketDetailConfiguration : IEntityTypeConfiguration<BlanketDetail>
{
    public void Configure(EntityTypeBuilder<BlanketDetail> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("BlanketDetail", "Purchase");
        b.HasKey(x => x.Id);

        // Quantity precision (18,3)
        b.Property(x => x.EstimatedQuantity).HasPrecision(18, 3);
        b.Property(x => x.Rate).HasPrecision(18, 3);

        // Value precision (18,2)
        b.Property(x => x.TotalPrice).HasPrecision(18, 2);

        // Percentage precision (5,2)
        b.Property(x => x.GSTPercentage).HasPrecision(5, 2);

        b.Property(x => x.QualitySpecification).HasMaxLength(500).IsRequired(false);

        b.Property(x => x.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        b.Property(x => x.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // FK — BlanketHeaderId → BlanketHeader (parent-child, cascade)
        b.HasOne(x => x.BlanketHeader)
            .WithMany(m => m.Details)
            .HasForeignKey(x => x.BlanketHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.BlanketHeaderId);
        b.HasIndex(x => x.ItemId);
    }
}
