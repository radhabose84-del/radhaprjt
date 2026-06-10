using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.FreightRfq;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.FreightRfq;

public class FreightRfqQuotationConfiguration : IEntityTypeConfiguration<FreightRfqQuotation>
{
    public void Configure(EntityTypeBuilder<FreightRfqQuotation> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("FreightRfqQuotation", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.FreightRfqHeaderId).IsRequired();
        b.Property(x => x.TransporterId).IsRequired();
        b.Property(x => x.RateBasisId).IsRequired();
        b.Property(x => x.QuotedRate).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(x => x.NoOfVehicles).IsRequired(false);
        b.Property(x => x.FreightValue).HasColumnType("decimal(18,2)").IsRequired();

        b.Property(x => x.IsSelected).HasColumnType("bit").IsRequired();
        b.Property(x => x.IsOverride).HasColumnType("bit").IsRequired();
        b.Property(x => x.Remarks).HasColumnType("varchar(500)").IsRequired(false);

        // FK: Rate Basis -> Purchase.MiscMaster (same module, no inverse navigation)
        b.HasOne<MiscMaster>()
         .WithMany()
         .HasForeignKey(x => x.RateBasisId)
         .OnDelete(DeleteBehavior.Restrict)
         .HasConstraintName("FK_FreightRfqQuotation_RateBasis");

        // TransporterId -> no DB FK (cross-module Party)

        b.HasIndex(x => x.FreightRfqHeaderId).HasDatabaseName("IX_FreightRfqQuotation_HeaderId");

        // BaseEntity columns
        b.Property(x => x.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
        b.Property(x => x.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int").IsRequired();
        b.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int").IsRequired(false);
        b.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate").HasColumnType("datetimeoffset").IsRequired(false);
        b.Property(x => x.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(50)").IsRequired(false);
        b.Property(x => x.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)").IsRequired(false);
    }
}
