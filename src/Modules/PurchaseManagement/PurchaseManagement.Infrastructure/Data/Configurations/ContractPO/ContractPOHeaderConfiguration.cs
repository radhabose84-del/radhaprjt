using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.ContractPO;

public class ContractPOHeaderConfiguration : IEntityTypeConfiguration<ContractPOHeader>
{
    public void Configure(EntityTypeBuilder<ContractPOHeader> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("ContractPOHeader", "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.ContractPONumber).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.ContractPONumber).IsUnique();

        b.Property(x => x.Remarks).HasMaxLength(500).IsRequired(false);

        b.Property(x => x.TotalContractValue).HasPrecision(18, 2);
        b.Property(x => x.UtilizedValue).HasPrecision(18, 2);
        b.Property(x => x.BalanceValue).HasPrecision(18, 2);

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
            .WithMany(m => m.ContractPOStatuses)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        b.HasIndex(x => x.VendorId);
        b.HasIndex(x => x.UnitId);
        b.HasIndex(x => x.StatusId);
    }
}
