using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            builder.ToTable("Location", "FixedAsset");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(ag => ag.Code)
            .HasColumnName("Code")
            .HasColumnType("varchar(10)")
            .IsRequired();

            builder.Property(ag => ag.LocationName)
            .HasColumnName("LocationName")
            .HasColumnType("varchar(50)")
            .IsRequired();

            builder.Property(ag => ag.Description)
           .HasColumnName("Description")
           .HasColumnType("varchar(250)");

            builder.Property(ag => ag.SortOrder)
            .HasColumnName("SortOrder")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(d => d.UnitId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(d => d.DepartmentId)
            .IsRequired()
            .HasColumnType("int");

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


        }
    }
}