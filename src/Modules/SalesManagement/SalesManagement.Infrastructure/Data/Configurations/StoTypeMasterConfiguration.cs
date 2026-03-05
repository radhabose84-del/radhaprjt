using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StoTypeMasterConfiguration : IEntityTypeConfiguration<StoTypeMaster>
    {
        public void Configure(EntityTypeBuilder<StoTypeMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("StoTypeMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoTypeCode)
                .HasColumnName("StoTypeCode")
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.StoTypeName)
                .HasColumnName("StoTypeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            builder.Property(t => t.PgiMovementTypeId)
                .HasColumnName("PgiMovementTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GrMovementTypeId)
                .HasColumnName("GrMovementTypeId")
                .HasColumnType("int")
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.StoTypeCode).IsUnique();
            builder.HasIndex(t => t.PgiMovementTypeId);
            builder.HasIndex(t => t.GrMovementTypeId);

            // FK: StoTypeMaster → MovementTypeConfig (PGI) — same module
            builder.HasOne(t => t.PgiMovementType)
                .WithMany(m => m.StoTypeMastersAsPgi)
                .HasForeignKey(t => t.PgiMovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: StoTypeMaster → MovementTypeConfig (GR) — same module
            builder.HasOne(t => t.GrMovementType)
                .WithMany(m => m.StoTypeMastersAsGr)
                .HasForeignKey(t => t.GrMovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
