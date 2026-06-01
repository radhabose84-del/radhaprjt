using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
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

            builder.ToTable("MiscMaster", "AppData");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.MiscTypeId)
                .HasColumnName("MiscTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.Code)
                   .HasColumnName("Code")
                   .HasColumnType("nvarchar(50)")
                   .IsRequired();

            builder.Property(m => m.Description)
                   .HasColumnName("description")
                   .HasColumnType("varchar(250)")
                   .IsRequired();

            builder.Property(m => m.SortOrder)
                   .HasColumnName("sortOrder")
                   .HasColumnType("int")
                   .HasDefaultValue(0)
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

            var dateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTimeOffset>(
                    v => new DateTimeOffset(v, TimeSpan.Zero),
                    v => v.DateTime);

            builder.Property(b => b.CreatedAt)
                    .HasColumnName("CreatedDate")
                    .HasColumnType("datetimeoffset")
                    .HasConversion(dateTimeOffsetConverter);

            builder.Property(b => b.ModifiedAt)
                    .HasColumnName("ModifiedDate")
                    .HasColumnType("datetimeoffset")
                    .HasConversion(dateTimeOffsetConverter);

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");


            builder.HasOne(m => m.MiscTypeMaster)
                .WithMany(t => t.MiscMaster)
                .HasForeignKey(m => m.MiscTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 👇 Explicit inverse navigation for CustomField (clarifies multi relationship use)
            builder.HasMany(m => m.CustomFieldDataTypes)
                .WithOne(cf => cf.DataType)
                .HasForeignKey(cf => cf.DataTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.CustomFieldLabelTypes)
                .WithOne(cf => cf.LabelType)
                .HasForeignKey(cf => cf.LabelTypeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}