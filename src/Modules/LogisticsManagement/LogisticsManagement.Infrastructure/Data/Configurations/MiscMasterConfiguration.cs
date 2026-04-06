using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LogisticsManagement.Domain.Entities;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.Infrastructure.Data.Configurations
{
    public class MiscMasterConfiguration : IEntityTypeConfiguration<MiscMaster>
    {
        public void Configure(EntityTypeBuilder<MiscMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("MiscMaster", "Logistics");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MiscTypeId)
                .HasColumnName("MiscTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Code)
                .HasColumnName("Code")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(t => t.SortOrder)
                .HasColumnName("SortOrder")
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

            builder.HasIndex(t => new { t.MiscTypeId, t.Code }).IsUnique();
            builder.HasIndex(t => t.MiscTypeId);

            builder.HasOne(t => t.MiscTypeMaster)
                .WithMany(mt => mt.MiscMasters)
                .HasForeignKey(t => t.MiscTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
