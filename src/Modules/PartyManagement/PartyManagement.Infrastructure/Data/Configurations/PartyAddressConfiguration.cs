using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyAddressConfiguration : IEntityTypeConfiguration<PartyAddress>
    {
        public void Configure(EntityTypeBuilder<PartyAddress> builder)
        {
            builder.ToTable("PartyAddress", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.PartyId)  // Foreign Key column
               .HasColumnName("PartyId")
               .HasColumnType("int")  // Set as int
               .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PartyAddressId)
                .WithMany(t => t.PartyAddressTypes)
                .HasForeignKey(m => m.PartyId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.AddressType)
               .HasColumnName("AddressType")
               .HasColumnType("nvarchar(25)");

            builder.Property(m => m.AddressLine1)
               .HasColumnName("AddressLine1")
               .HasColumnType("nvarchar(100)");

            builder.Property(m => m.AddressLine2)
               .HasColumnName("AddressLine2")
               .HasColumnType("nvarchar(100)");

            builder.Property(m => m.CityId)
               .HasColumnName("CityId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.StateId)
               .HasColumnName("StateId")
               .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.PostalCode)
               .HasColumnName("PostalCode")
               .HasColumnType("nvarchar(10)");

            builder.Property(m => m.CountryId)
               .HasColumnName("CountryId")
                .HasColumnType("int")
                .IsRequired();

            // Cross-module FK to AppData.Location — no DB constraint, optional (Ginner addresses).
            builder.Property(m => m.LocationId)
               .HasColumnName("LocationId")
               .HasColumnType("int")
               .IsRequired(false);

            // Cross-module FK to AppData.Station — no DB constraint, optional.
            builder.Property(m => m.StationId)
               .HasColumnName("StationId")
               .HasColumnType("int")
               .IsRequired(false);
        }
    }
}