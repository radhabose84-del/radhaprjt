using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOfficeConfiguration : IEntityTypeConfiguration<SalesOffice>
    {
        public void Configure(EntityTypeBuilder<SalesOffice> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesOffice", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOfficeName)
                .HasColumnName("SalesOfficeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.SalesOrganisationId)
                .HasColumnName("SalesOrganisationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CityId)
                .HasColumnName("CityId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Pincode)
                .HasColumnName("Pincode")
                .HasColumnType("varchar(20)")
                .IsRequired(false);

            builder.Property(t => t.Phone)
                .HasColumnName("Phone")
                .HasColumnType("varchar(20)")
                .IsRequired(false);

            builder.Property(t => t.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(t => t.ResponsibleManager)
                .HasColumnName("ResponsibleManager")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.RegionTerritory)
                .HasColumnName("RegionTerritory")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.Address)
                .HasColumnName("Address")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

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

            // Composite unique index: SalesOfficeName unique within SalesOrganisation (BR-2)
            builder.HasIndex(t => new { t.SalesOrganisationId, t.SalesOfficeName }).IsUnique();
            builder.HasIndex(t => t.SalesOrganisationId);
            builder.HasIndex(t => t.CityId);

            // FK: SalesOffice → SalesOrganisation (same module, Sales schema)
            builder.HasOne(t => t.SalesOrganisation)
                .WithMany(o => o.SalesOffices)
                .HasForeignKey(t => t.SalesOrganisationId)
                .OnDelete(DeleteBehavior.Restrict);

            // CityId is cross-module (UserManagement) — no FK constraint
        }
    }
}
