using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ImportPO;

public class ImportPODetailConfiguration : IEntityTypeConfiguration<ImportPODetail>
{
    public void Configure(EntityTypeBuilder<ImportPODetail> b)
    {
        b.ToTable("PurchaseOrderImportDetail", "Purchase");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.FreightAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsuranceAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.CIFValue).HasColumnType("decimal(18,2)");
        b.Property(x => x.BasicCustomDuty).HasColumnType("decimal(18,2)");
        b.Property(x => x.SocialWelfareSurCharges).HasColumnType("decimal(18,2)");
        b.Property(x => x.IGST).HasColumnType("decimal(18,2)");
        b.Property(x => x.AgriInfraDevCess).HasColumnType("decimal(18,2)");
        b.Property(x => x.AntiDumpingDuty).HasColumnType("decimal(18,2)");
        b.Property(x => x.SafeguardDuty).HasColumnType("decimal(18,2)");
        b.Property(x => x.HealthEducationCess).HasColumnType("decimal(18,2)");
        b.Property(x => x.OtherCharges).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalValue).HasColumnType("decimal(18,2)");
        b.Property(x => x.DutyMasterId).IsRequired(false);
        b.Property(x => x.GRBasedIV).HasColumnName("GRBasedIV"); 

        b.HasIndex(x => x.PurchaseHeaderId);

        b.HasOne(x => x.Header)
        .WithMany(m => m.ImportPODetails)
        .HasForeignKey(x => x.PurchaseHeaderId)
        .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.dutyMaster)
        .WithMany(m => m.ImportPODuty)
        .HasForeignKey(x => x.DutyMasterId)
        .OnDelete(DeleteBehavior.NoAction);
    }
}
