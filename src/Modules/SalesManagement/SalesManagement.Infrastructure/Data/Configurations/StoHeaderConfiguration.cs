using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StoHeaderConfiguration : IEntityTypeConfiguration<StoHeader>
    {
        public void Configure(EntityTypeBuilder<StoHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("StoHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoNumber)
                .HasColumnName("StoNumber")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.DocumentDate)
                .HasColumnName("DocumentDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ExpectedDeliveryDate)
                .HasColumnName("ExpectedDeliveryDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.StoTypeId)
                .HasColumnName("StoTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MovementTypeId)
                .HasColumnName("MovementTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SupplyingPlantId)
                .HasColumnName("SupplyingPlantId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SupplyingStorageLocationId)
                .HasColumnName("SupplyingStorageLocationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingPlantId)
                .HasColumnName("ReceivingPlantId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingStorageLocationId)
                .HasColumnName("ReceivingStorageLocationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(t => t.HeaderStatusId)
                .HasColumnName("HeaderStatusId")
                .HasColumnType("int")
                .IsRequired(false);

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

            // Same-module FK: StoHeader → StoTypeMaster
            builder.HasOne(t => t.StoType)
                .WithMany(st => st.StoHeaders)
                .HasForeignKey(t => t.StoTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: StoHeader → MovementTypeConfig
            builder.HasOne(t => t.MovementType)
                .WithMany(m => m.StoHeaders)
                .HasForeignKey(t => t.MovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: StoHeader → MiscMaster (HeaderStatus)
            builder.HasOne(t => t.HeaderStatus)
                .WithMany(m => m.StoHeadersAsHeaderStatus)
                .HasForeignKey(t => t.HeaderStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection: StoHeader → StoDetails
            builder.HasMany(t => t.StoDetails)
                .WithOne(d => d.StoHeader)
                .HasForeignKey(d => d.StoHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.StoNumber).IsUnique();
            builder.HasIndex(t => t.StoTypeId);
            builder.HasIndex(t => t.MovementTypeId);
            builder.HasIndex(t => t.SupplyingPlantId);
            builder.HasIndex(t => t.ReceivingPlantId);
            builder.HasIndex(t => t.HeaderStatusId);
            builder.HasIndex(t => t.DocumentDate);
        }
    }
}
