using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartyManagement.Domain.Entities;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class TransportDetailConfiguration : IEntityTypeConfiguration<TransportDetail>
    {
        public void Configure(EntityTypeBuilder<TransportDetail> builder)
        {
            builder.ToTable("TransportDetail", "Party");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TransporterTypeId)
                .HasColumnName("TransporterTypeId")
                .HasColumnType("int");

            builder.Property(t => t.TransportModeId)
                .HasColumnName("TransportModeId")
                .HasColumnType("int");

            builder.Property(t => t.VehicleTypeId)
                .HasColumnName("VehicleTypeId")
                .HasColumnType("int");

            builder.Property(t => t.DefaultFreightTypeId)
                .HasColumnName("DefaultFreightTypeId")
                .HasColumnType("int");

            builder.Property(t => t.DefaultFreightRate)
                .HasColumnName("DefaultFreightRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.MinFreightAmount)
                .HasColumnName("MinFreightAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.LicenseNo)
                .HasColumnName("LicenseNo")
                .HasColumnType("nvarchar(50)");

            builder.Property(t => t.LicenseExpiryDate)
                .HasColumnName("LicenseExpiryDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            builder.Property(t => t.VehicleNo)
                .HasColumnName("VehicleNo")
                .HasColumnType("nvarchar(50)");

            builder.Property(t => t.InsuranceProvider)
                .HasColumnName("InsuranceProvider")
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(t => t.PolicyNo)
                .HasColumnName("PolicyNo")
                .HasColumnType("nvarchar(50)")
                .IsRequired(false);

            builder.Property(t => t.InsuranceExpiryDate)
                .HasColumnName("InsuranceExpiryDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // Cross-module FK → UserManagement / AppData.Modules — nullable column, NO DB FK.
            builder.Property(t => t.ModuleId)
                .HasColumnName("ModuleId")
                .HasColumnType("int")
                .IsRequired(false);

            // Same-module FK → Party.MiscMaster, nullable.
            builder.Property(t => t.DefaultProcurementRateBasisId)
                .HasColumnName("DefaultProcurementRateBasisId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Status)
                .HasColumnName("Status")
                .HasColumnType("tinyint")
                .HasDefaultValue((byte)1)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.PartyId)
                .HasDatabaseName("IX_TransportDetail_PartyId");

            builder.HasIndex(t => t.TransporterTypeId)
                .HasDatabaseName("IX_TransportDetail_TransporterTypeId");

            builder.HasIndex(t => t.TransportModeId)
                .HasDatabaseName("IX_TransportDetail_TransportModeId");

            builder.HasIndex(t => t.VehicleTypeId)
                .HasDatabaseName("IX_TransportDetail_VehicleTypeId");

            builder.HasIndex(t => t.DefaultFreightTypeId)
                .HasDatabaseName("IX_TransportDetail_DefaultFreightTypeId");

            // Indexes for the two new nullable FKs.
            builder.HasIndex(t => t.ModuleId)
                .HasDatabaseName("IX_TransportDetail_ModuleId");

            builder.HasIndex(t => t.DefaultProcurementRateBasisId)
                .HasDatabaseName("IX_TransportDetail_DefaultProcurementRateBasisId");

            // FK to PartyMaster
            builder.HasOne(t => t.PartyMaster)
                .WithMany(p => p.TransportDetails)
                .HasForeignKey(t => t.PartyId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK to MiscMaster (TransporterType)
            builder.HasOne(t => t.TransporterTypeMisc)
                .WithMany(m => m.TransportDetailTransporterType)
                .HasForeignKey(t => t.TransporterTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK to MiscMaster (TransportMode)
            builder.HasOne(t => t.TransportModeMisc)
                .WithMany(m => m.TransportDetailTransportMode)
                .HasForeignKey(t => t.TransportModeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK to MiscMaster (VehicleType)
            builder.HasOne(t => t.VehicleTypeMisc)
                .WithMany(m => m.TransportDetailVehicleType)
                .HasForeignKey(t => t.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK to MiscMaster (DefaultFreightType)
            builder.HasOne(t => t.DefaultFreightTypeMisc)
                .WithMany(m => m.TransportDetailDefaultFreightType)
                .HasForeignKey(t => t.DefaultFreightTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK to MiscMaster (DefaultProcurementRateBasis) — optional, so IsRequired(false)
            // makes both the FK column and the navigation accept null.
            builder.HasOne(t => t.DefaultProcurementRateBasisMisc)
                .WithMany(m => m.TransportDetailDefaultProcurementRateBasis)
                .HasForeignKey(t => t.DefaultProcurementRateBasisId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ModuleId is cross-module (UserManagement / AppData.Modules) — no DB FK constraint
            // per BSOFT cross-module rule; integrity is enforced via IModuleLookup at the API layer.
        }
    }
}
