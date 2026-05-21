using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.ContractPO;

public class ContractPOReleaseHistoryConfiguration : IEntityTypeConfiguration<ContractPOReleaseHistory>
{
    public void Configure(EntityTypeBuilder<ContractPOReleaseHistory> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("ContractPOReleaseHistory", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.ReleasedQuantity).HasPrecision(18, 3);
        b.Property(x => x.ReleasedRate).HasPrecision(18, 2);
        b.Property(x => x.ReleasedValue).HasPrecision(18, 2);

        b.Property(x => x.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        b.Property(x => x.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // FK — ContractPOHeaderId → ContractPOHeader
        b.HasOne(x => x.ContractPOHeader)
            .WithMany(m => m.ContractPOReleaseHistories)
            .HasForeignKey(x => x.ContractPOHeaderId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — ContractPODetailId → ContractPODetail
        b.HasOne(x => x.ContractPODetail)
            .WithMany(m => m.ContractPOReleaseHistories)
            .HasForeignKey(x => x.ContractPODetailId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK — ReleasePOId → PurchaseOrderHeader
        b.HasOne(x => x.ReleasePurchaseOrder)
            .WithMany(m => m.ContractPOReleaseHistories)
            .HasForeignKey(x => x.ReleasePOId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        b.HasIndex(x => x.ContractPOHeaderId);
        b.HasIndex(x => x.ContractPODetailId);
        b.HasIndex(x => x.ReleasePOId);
    }
}
