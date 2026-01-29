using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Data.Configurations
{
    public class SubLocationConfiguration : IEntityTypeConfiguration<SubLocation>
    {
        public void Configure(EntityTypeBuilder<SubLocation> builder)
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

            builder.ToTable("SubLocation", "FixedAsset");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(b => b.Code)
            .HasColumnName("Code")
            .HasColumnType("varchar(10)")
            .IsRequired();

            builder.Property(b => b.SubLocationName)
            .HasColumnName("SubLocationName")
            .HasColumnType("varchar(50)")
            .IsRequired();

            builder.Property(b => b.Description)
           .HasColumnName("Description")
           .HasColumnType("varchar(250)");

            builder.Property(d => d.UnitId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(d => d.DepartmentId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(u => u.LocationId)
            .HasColumnName("LocationId")
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

            // Configure the foreign key relationship
            builder.HasOne(ag => ag.Location)   // Each SubLocation belongs to one Location
            .WithMany(l => l.SubLocations)  // One Location can have many SubLocations
            .HasForeignKey(ua => ua.LocationId)
            .OnDelete(DeleteBehavior.Cascade);


        }
    }
}