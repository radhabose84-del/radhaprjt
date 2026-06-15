using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class AccessPolicyConfiguration : IEntityTypeConfiguration<AccessPolicy>
    {
        public void Configure(EntityTypeBuilder<AccessPolicy> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("AccessPolicy", "AppSecurity");
            builder.HasKey(ap => ap.Id);
            builder.Property(ap => ap.Id).ValueGeneratedOnAdd();

            builder.Property(ap => ap.PolicyCode)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(ap => ap.PolicyName)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(ap => ap.EntityName)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(ap => ap.FieldName)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(ap => ap.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(ap => ap.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeletedConverter)
                .IsRequired();

            builder.Property(ap => ap.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(ap => ap.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("datetime2");

            builder.Property(ap => ap.CreatedByName)
                .HasColumnName("CreatedByName")
                .HasColumnType("varchar(50)");

            builder.Property(ap => ap.CreatedIP)
                .HasColumnName("CreatedIP")
                .HasColumnType("varchar(255)");

            builder.Property(ap => ap.ModifiedBy)
                .HasColumnName("ModifiedBy")
                .HasColumnType("int");

            builder.Property(ap => ap.ModifiedAt)
                .HasColumnName("ModifiedAt")
                .HasColumnType("datetime2");

            builder.Property(ap => ap.ModifiedByName)
                .HasColumnName("ModifiedByName")
                .HasColumnType("varchar(50)");

            builder.Property(ap => ap.ModifiedIP)
                .HasColumnName("ModifiedIP")
                .HasColumnType("varchar(255)");

            builder.HasIndex(ap => ap.PolicyCode).IsUnique();
        }
    }
}
