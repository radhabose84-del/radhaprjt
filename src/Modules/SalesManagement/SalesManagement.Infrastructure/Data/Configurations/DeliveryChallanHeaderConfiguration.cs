using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DeliveryChallanHeaderConfiguration : IEntityTypeConfiguration<DeliveryChallanHeader>
    {
        public void Configure(EntityTypeBuilder<DeliveryChallanHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DeliveryChallanHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DeliveryNumber)
                .HasColumnName("DeliveryNumber")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.DeliveryDate)
                .HasColumnName("DeliveryDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.StoHeaderId)
                .HasColumnName("StoHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FromPlantId)
                .HasColumnName("FromPlantId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FromStorageLocationId)
                .HasColumnName("FromStorageLocationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ToPlantId)
                .HasColumnName("ToPlantId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ToStorageLocationId)
                .HasColumnName("ToStorageLocationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TransporterId)
                .HasColumnName("TransporterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.VehicleNumber)
                .HasColumnName("VehicleNumber")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.TransportDistance)
                .HasColumnName("TransportDistance")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.DeliveryValue)
                .HasColumnName("DeliveryValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ConsignmentValue)
                .HasColumnName("ConsignmentValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DcTypeId)
                .HasColumnName("DcTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MovementTypeId)
                .HasColumnName("MovementTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(t => t.GEFlag)
                .HasColumnName("GEFlag")
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValue(false);

            // Status & Audit
            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK: DeliveryChallanHeader → StoHeader
            builder.HasOne(t => t.StoHeader)
                .WithMany(s => s.DeliveryChallanHeaders)
                .HasForeignKey(t => t.StoHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: DeliveryChallanHeader → MiscMaster (Status)
            builder.HasOne(t => t.Status)
                .WithMany(m => m.DeliveryChallanHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: DeliveryChallanHeader → MiscMaster (DcType)
            builder.HasOne(t => t.DcType)
                .WithMany(m => m.DeliveryChallanHeadersAsDcType)
                .HasForeignKey(t => t.DcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: DeliveryChallanHeader → MovementTypeConfig
            builder.HasOne(t => t.MovementType)
                .WithMany(m => m.DeliveryChallanHeaders)
                .HasForeignKey(t => t.MovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection: DeliveryChallanHeader → DeliveryChallanDetails
            builder.HasMany(t => t.DeliveryChallanDetails)
                .WithOne(d => d.DeliveryChallanHeader)
                .HasForeignKey(d => d.DeliveryChallanHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.DeliveryNumber).IsUnique();
            builder.HasIndex(t => t.StoHeaderId);
            builder.HasIndex(t => t.FromPlantId);
            builder.HasIndex(t => t.ToPlantId);
            builder.HasIndex(t => t.TransporterId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.DcTypeId);
            builder.HasIndex(t => t.MovementTypeId);
            builder.HasIndex(t => t.DeliveryDate);
        }
    }
}
