using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster;

public class BlanketScheduleConfiguration : IEntityTypeConfiguration<BlanketSchedule>
{
    public void Configure(EntityTypeBuilder<BlanketSchedule> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("BlanketSchedule", "Purchase");
        b.HasKey(x => x.Id);

        // Quantity precision (18,3)
        b.Property(x => x.ScheduleQuantity).HasPrecision(18, 3);

        b.Property(x => x.Remarks).HasMaxLength(500).IsRequired(false);

        b.Property(x => x.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        b.Property(x => x.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // FK — BlanketDetailId → BlanketDetail (parent-child, cascade)
        b.HasOne(x => x.BlanketDetail)
            .WithMany(m => m.Schedules)
            .HasForeignKey(x => x.BlanketDetailId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.BlanketDetailId);
    }
}
