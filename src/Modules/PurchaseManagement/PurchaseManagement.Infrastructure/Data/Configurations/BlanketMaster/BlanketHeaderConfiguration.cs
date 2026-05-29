using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster;

public class BlanketHeaderConfiguration : IEntityTypeConfiguration<BlanketHeader>
{
    public void Configure(EntityTypeBuilder<BlanketHeader> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("BlanketHeader", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.BlanketNumber).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.BlanketNumber).IsUnique();

        b.Property(x => x.BrokerName).HasMaxLength(200).IsRequired(false);
        b.Property(x => x.PaymentTerms).HasMaxLength(500).IsRequired(false);
        b.Property(x => x.DeliveryTerms).HasMaxLength(500).IsRequired(false);
        b.Property(x => x.Remarks).HasMaxLength(500).IsRequired(false);

        b.Property(x => x.TotalEstimatedValue).HasPrecision(18, 2);

        b.Property(x => x.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        b.Property(x => x.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // FK — StatusId → MiscMaster (same-module)
        b.HasOne(x => x.MiscStatus)
            .WithMany(m => m.BlanketStatuses)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — ProcurementTypeId → MiscMaster (same-module)
        b.HasOne(x => x.MiscProcurementType)
            .WithMany(m => m.BlanketProcurementTypes)
            .HasForeignKey(x => x.ProcurementTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        b.HasIndex(x => x.VendorId);
        b.HasIndex(x => x.UnitId);
        b.HasIndex(x => x.StatusId);
        b.HasIndex(x => x.ProcurementTypeId);
    }
}
