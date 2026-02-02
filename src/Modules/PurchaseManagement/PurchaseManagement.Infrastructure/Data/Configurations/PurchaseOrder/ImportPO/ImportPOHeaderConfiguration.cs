using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ImportPO;

public class ImportPOHeaderConfiguration : IEntityTypeConfiguration<ImportPOHeader>
{
    public void Configure(EntityTypeBuilder<ImportPOHeader> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
              v => v == Status.Active,                    // Convert to DB (1 for Active)
              v => v ? Status.Active : Status.Inactive    // Convert to Entity
          );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );
        b.ToTable("PurchaseOrderImportHeader", "Purchase");

        b.HasKey(x => x.Id);

        b.Property(x => x.ExpectedTimeOfDeparture).IsRequired();
        b.Property(x => x.ExpectedTimeOfArrival).IsRequired();
        b.Property(x => x.BillOfEntryNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.TTExchangeRateId).IsRequired(false);
        b.Property(x => x.ShippingPortId).IsRequired(false);
        b.Property(x => x.CustomsOfficeId).IsRequired(false);
        b.Property(x => x.DestinationPortId).IsRequired(false);
        b.Property(x => x.OriginCountryId).IsRequired(false);
        b.Property(x => x.CustomsHouseAgentId).IsRequired(false);
        b.Property(x => x.BillOfEntryNumber).IsRequired(false);


        b.Property(x => x.BillOfLadingNumber).HasMaxLength(80);
        b.Property(x => x.VesselName).HasMaxLength(120);
        b.Property(x => x.ContainerNumber).HasMaxLength(40);
        b.Property(x => x.AirlineName).HasMaxLength(120);
        b.Property(x => x.AirWaybillNumber).HasMaxLength(40);
        b.Property(x => x.FlightNumber).HasMaxLength(40);
        b.Property(x => x.TTReferenceNumber).HasMaxLength(80);
        b.Property(x => x.LCNumber).HasMaxLength(80);
        b.Property(x => x.DemurrageTerms).HasMaxLength(512);
        b.Property(x => x.TTRemarks).HasMaxLength(1024);
        b.Property(x => x.LCRemarks).HasMaxLength(1024);
        b.Property(x => x.TTSwiftCode).HasMaxLength(30);
        b.Property(x => x.LCSwiftCode).HasMaxLength(30);

        b.Property(x => x.TTExchangeRate).HasColumnType("decimal(18,5)");
        b.Property(x => x.LCAmount).HasColumnType("decimal(18,2)");

        // Indexes you’ll likely want
                b.HasIndex(x => x.PurchaseOrderId);
        b.HasIndex(x => x.BillOfLadingNumber);
        b.HasIndex(x => x.AirWaybillNumber);
        b.HasIndex(x => new { x.ModeOfTransportId, x.ExpectedTimeOfDeparture });

        b.HasOne(x => x.ImportPurchase)
        .WithMany(m => m.ImportPOHeader)
        .HasForeignKey(x => x.PurchaseOrderId)
        .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.MiscIncoterms)
         .WithMany(m => m.importPOHeaderIncoterms)
         .HasForeignKey(x => x.IncotermId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.MOT)
        .WithMany(m => m.importPOHeaderMOT)
        .HasForeignKey(x => x.ModeOfTransportId)
        .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.ShippingMode)
        .WithMany(m => m.importPOHeaderShipMode)
        .HasForeignKey(x => x.ShippingModeId)
        .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.CustomsOffice)
        .WithMany(m => m.importPOHeaderCustomsOffice)
        .HasForeignKey(x => x.CustomsOfficeId)
        .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.ShipPort)
        .WithMany(m => m.importPOHeaderShipPort)
        .HasForeignKey(x => x.ShippingPortId)
        .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.DestPort)
        .WithMany(m => m.importPOHeaderDestPort)
        .HasForeignKey(x => x.DestinationPortId)
        .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.EXRate)
        .WithMany(m => m.importPOHeaderExRate)
        .HasForeignKey(x => x.TTExchangeRateId)
        .OnDelete(DeleteBehavior.Restrict);
        
        b.HasOne(x => x.LCPaymentStatus)
         .WithMany(m => m.ImportLCPayment)
         .HasForeignKey(x => x.LCPaymentStatusId)
         .OnDelete(DeleteBehavior.NoAction);
        
        b.HasOne(x => x.LCType)
         .WithMany(m => m.ImportLCType)
         .HasForeignKey(x => x.LCTypeId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.LCPaymentMode)
         .WithMany(m => m.ImportLCPaymentMode)
         .HasForeignKey(x => x.LCPaymentModeId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.TTPaymentMode)
         .WithMany(m => m.ImportTTPaymentMode)
         .HasForeignKey(x => x.TTPaymentModeId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.TTPaymentStatus)
         .WithMany(m => m.ImportTTPayment)
         .HasForeignKey(x => x.TTPaymentStatusId)
         .OnDelete(DeleteBehavior.NoAction);


        b.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

        b.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

        b.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

        b.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

        b.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

        b.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");
    }
}

