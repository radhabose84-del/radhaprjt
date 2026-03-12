using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public class ProcurementTypeConfiguration : IEntityTypeConfiguration<ProcurementType>
    {
        public void Configure(EntityTypeBuilder<ProcurementType> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ProcurementType", "Inventory");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProcurementCode)
                .HasColumnName("ProcurementCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.ProcurementName)
                .HasColumnName("ProcurementName")
                .HasColumnType("varchar(100)")
                .IsRequired();

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

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(255)");

            // Indexes
            builder.HasIndex(t => t.ProcurementCode).IsUnique();
        }
    }
}
