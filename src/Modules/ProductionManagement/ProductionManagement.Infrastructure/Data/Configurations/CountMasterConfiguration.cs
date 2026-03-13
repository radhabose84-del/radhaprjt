using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class CountMasterConfiguration : IEntityTypeConfiguration<CountMaster>
    {
        public void Configure(EntityTypeBuilder<CountMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CountMaster", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CountCode)
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.CountValue)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(t => t.ShortName)
                .HasColumnType("varchar(50)");

            builder.Property(t => t.CountCategoryId)
                .HasColumnType("int");

            builder.Property(t => t.CountTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CountDescription)
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(t => t.UOMId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int").IsRequired();
            builder.Property(t => t.CreatedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            builder.HasIndex(t => t.CountCode).IsUnique();
            builder.HasIndex(t => t.CountTypeId);
            builder.HasIndex(t => t.CountCategoryId);
            builder.HasIndex(t => t.UOMId);

            // FK: CountMaster → MiscMaster (same-module, CountTypeId)
            builder.HasOne(t => t.CountType)
                .WithMany(m => m.CountMastersAsCountType)
                .HasForeignKey(t => t.CountTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: CountMaster → MiscMaster (same-module, CountCategoryId — optional)
            builder.HasOne(t => t.CountCategory)
                .WithMany(m => m.CountMastersAsCountCategory)
                .HasForeignKey(t => t.CountCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
