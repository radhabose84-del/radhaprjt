using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserSignatureConfiguration : IEntityTypeConfiguration<UserSignature>
    {
        public void Configure(EntityTypeBuilder<UserSignature> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("UserSignature", "AppData");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(u => u.UserId)
                .HasColumnName("UserId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(u => u.FileName)
                .HasColumnName("FileName")
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.Property(u => u.OriginalFileName)
                .HasColumnName("OriginalFileName")
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.Property(u => u.FilePath)
                .HasColumnName("FilePath")
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(u => u.FileType)
                .HasColumnName("FileType")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(u => u.FileSize)
                .HasColumnName("FileSize")
                .HasColumnType("bigint")
                .IsRequired();

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

            // Filtered unique index — one active signature per user; soft-deleted rows excluded
            builder.HasIndex(u => u.UserId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_UserSignature_UserId");

            // Same-module FK with DB constraint
            builder.HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
