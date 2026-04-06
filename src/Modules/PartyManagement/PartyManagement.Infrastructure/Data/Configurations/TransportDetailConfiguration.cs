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

            builder.Property(t => t.Status)
                .HasColumnName("Status")
                .HasColumnType("tinyint")
                .HasDefaultValue((byte)1)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.PartyId)
                .HasDatabaseName("IX_TransportDetail_PartyId");

            builder.HasIndex(t => t.TransportModeId)
                .HasDatabaseName("IX_TransportDetail_TransportModeId");

            builder.HasIndex(t => t.VehicleTypeId)
                .HasDatabaseName("IX_TransportDetail_VehicleTypeId");

            builder.HasIndex(t => t.DefaultFreightTypeId)
                .HasDatabaseName("IX_TransportDetail_DefaultFreightTypeId");

            // FK to PartyMaster
            builder.HasOne(t => t.PartyMaster)
                .WithMany(p => p.TransportDetails)
                .HasForeignKey(t => t.PartyId)
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
        }
    }
}
