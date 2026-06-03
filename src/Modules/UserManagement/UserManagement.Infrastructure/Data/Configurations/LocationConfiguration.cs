using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>
            (
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeletedConverter = new ValueConverter<IsDelete, bool>
            (
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("Location", "AppData");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("Id")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(u => u.Code)
                   .HasColumnName("Code")
                   .HasColumnType("varchar(20)")
                   .IsRequired();

            builder.Property(u => u.LocationName)
                   .HasColumnName("LocationName")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(u => u.Description)
                   .HasColumnName("Description")
                   .HasColumnType("varchar(250)")
                   .IsRequired(false);

            builder.Property(u => u.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("bit")
                   .HasConversion(isActiveConverter)
                   .IsRequired();

            builder.Property(u => u.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasColumnType("bit")
                   .HasConversion(isDeletedConverter)
                   .IsRequired();

            builder.Property(u => u.CreatedBy)
                   .HasColumnName("CreatedBy")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("CreatedDate")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(u => u.CreatedByName)
                   .HasColumnName("CreatedByName")
                   .HasColumnType("varchar(100)")
                   .IsRequired(false);

            builder.Property(u => u.CreatedIP)
                   .HasColumnName("CreatedIP")
                   .HasColumnType("varchar(50)")
                   .IsRequired(false);

            builder.Property(u => u.ModifiedBy)
                   .HasColumnName("ModifiedBy")
                   .HasColumnType("int")
                   .IsRequired(false);

            builder.Property(u => u.ModifiedAt)
                   .HasColumnName("ModifiedDate")
                   .HasColumnType("datetime")
                   .IsRequired(false);

            builder.Property(u => u.ModifiedByName)
                   .HasColumnName("ModifiedByName")
                   .HasColumnType("varchar(100)")
                   .IsRequired(false);

            builder.Property(u => u.ModifiedIP)
                   .HasColumnName("ModifiedIP")
                   .HasColumnType("varchar(50)")
                   .IsRequired(false);

            builder.HasIndex(u => u.Code).IsUnique();
        }
    }
}
