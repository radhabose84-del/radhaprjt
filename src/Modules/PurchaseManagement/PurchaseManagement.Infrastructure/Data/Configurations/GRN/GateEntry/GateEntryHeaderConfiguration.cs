using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.GRN.GateEntry
{
    public class GateEntryHeaderConfiguration : IEntityTypeConfiguration<GateEntryHeader>
    {
        public void Configure(EntityTypeBuilder<GateEntryHeader> builder)
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

            builder.ToTable("GateEntryHeader", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.GateEntryNo)
                   .HasColumnName("GateEntryNo")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(b => b.GateEntryDate)
                    .HasColumnName("GateEntryDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");


            builder.Property(m => m.UnitId)
                   .HasColumnName("UnitId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.ReceivingTypeId)
                .HasColumnName("ReceivingTypeId")
                .HasColumnType("int")
                .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GateEntryReceivingType)
                .WithMany(t => t.GateEntryReceived)
                .HasForeignKey(m => m.ReceivingTypeId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed


            builder.Property(m => m.PartyId)
                   .HasColumnName("PartyId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(m => m.VehicleNumber)
                   .HasColumnName("VehicleNumber")
                   .HasColumnType("nvarchar(100)");

            builder.Property(m => m.DriverName)
                  .HasColumnName("DriverName")
                  .HasColumnType("nvarchar(100)");


            builder.Property(dg => dg.GrossWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false)
                .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(dg => dg.TareWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false)
                .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(dg => dg.NetWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false)
                .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(m => m.ImagePath)
                 .HasColumnName("ImagePath")
                 .HasColumnType("nvarchar(200)");

            builder.Property(m => m.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(250)");

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
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");

            // Ignore the reverse-nav collection — the FK relationship to GrnHeader was dropped
            // when GrnHeader.GateEntryId became a nullable, unconstrained column. Without this,
            // EF Core would try to generate a shadow FK to keep the navigation valid.
            builder.Ignore(b => b.GrnGateEntries);

        }
    }
}