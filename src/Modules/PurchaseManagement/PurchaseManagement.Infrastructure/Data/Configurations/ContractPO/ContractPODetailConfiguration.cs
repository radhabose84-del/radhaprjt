using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.ContractPO;

public class ContractPODetailConfiguration : IEntityTypeConfiguration<ContractPODetail>
{
    public void Configure(EntityTypeBuilder<ContractPODetail> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("ContractPODetail", "Purchase");
        b.HasKey(x => x.Id);

        // Quantity precision (18,3)
        b.Property(x => x.ContractQuantity).HasPrecision(18, 3);
        b.Property(x => x.UtilizedQuantity).HasPrecision(18, 3);
        b.Property(x => x.BalanceQuantity).HasPrecision(18, 3);

        // Value precision (18,2)
        b.Property(x => x.ContractRate).HasPrecision(18, 2);
        b.Property(x => x.ContractValue).HasPrecision(18, 2);
        b.Property(x => x.UtilizedValue).HasPrecision(18, 2);
        b.Property(x => x.BalanceValue).HasPrecision(18, 2);

        b.Property(x => x.GSTPercentage).HasPrecision(5, 2);

        b.Property(x => x.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        b.Property(x => x.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // FK — ContractPOHeaderId → ContractPOHeader (parent-child, cascade)
        b.HasOne(x => x.ContractPOHeader)
            .WithMany(m => m.ContractPODetails)
            .HasForeignKey(x => x.ContractPOHeaderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.ContractPOHeaderId);
        b.HasIndex(x => x.ItemId);
    }
}
