using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DispatchAddressMasterConfiguration : IEntityTypeConfiguration<DispatchAddressMaster>
    {
        public void Configure(EntityTypeBuilder<DispatchAddressMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DispatchAddressMaster", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchAddressName)
                .HasColumnName("DispatchAddressName")
                .HasColumnType("varchar(150)")
                .IsRequired();

            builder.Property(t => t.AddressLine1)
                .HasColumnName("AddressLine1")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(t => t.AddressLine2)
                .HasColumnName("AddressLine2")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            builder.Property(t => t.CityId)
                .HasColumnName("CityId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StateId)
                .HasColumnName("StateId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CountryId)
                .HasColumnName("CountryId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PinCode)
                .HasColumnName("PinCode")
                .HasColumnType("varchar(6)")
                .IsRequired();

            builder.Property(t => t.ContactPerson)
                .HasColumnName("ContactPerson")
                .HasColumnType("varchar(120)")
                .IsRequired(false);

            builder.Property(t => t.MobileNumber)
                .HasColumnName("MobileNumber")
                .HasColumnType("varchar(10)")
                .IsRequired(false);

            builder.Property(t => t.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar(254)")
                .IsRequired(false);

            builder.Property(t => t.GSTIN)
                .HasColumnName("GSTIN")
                .HasColumnType("varchar(15)")
                .IsRequired(false);

            builder.Property(t => t.Latitude)
                .HasColumnName("Latitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired(false);

            builder.Property(t => t.Longitude)
                .HasColumnName("Longitude")
                .HasColumnType("decimal(9,6)")
                .IsRequired(false);

            builder.Property(t => t.FreightId)
                .HasColumnName("FreightId")
                .HasColumnType("int")
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

            // Composite index to support duplicate-check query (Name + CityId + PinCode)
            // Not a unique DB constraint — uniqueness is enforced at application layer
            // to correctly handle soft-deleted records with the same combination
            builder.HasIndex(t => new { t.DispatchAddressName, t.CityId, t.PinCode })
                .HasDatabaseName("IX_DispatchAddressMaster_Composite");

            // FK support indexes (cross-module — no DB FK constraints)
            builder.HasIndex(t => t.CityId)
                .HasDatabaseName("IX_DispatchAddressMaster_CityId");

            builder.HasIndex(t => t.StateId)
                .HasDatabaseName("IX_DispatchAddressMaster_StateId");

            builder.HasIndex(t => t.CountryId)
                .HasDatabaseName("IX_DispatchAddressMaster_CountryId");

            builder.HasIndex(t => t.FreightId)
                .HasDatabaseName("IX_DispatchAddressMaster_FreightId");

            // Same-module FK to FreightMaster
            builder.HasOne(t => t.FreightMaster)
                .WithMany(f => f.DispatchAddressMasters)
                .HasForeignKey(t => t.FreightId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
