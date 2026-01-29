using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UnitAddressConfiguration : IEntityTypeConfiguration<UnitAddress>
    {
        public void Configure(EntityTypeBuilder<UnitAddress> builder)
        {
        builder.ToTable("UnitAddress", "AppData");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.CountryId)
            .HasColumnName("CountryId")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.StateId)
            .HasColumnName("StateId")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.CityId)
            .HasColumnName("CityId")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.AddressLine1)
            .HasColumnName("AddressLine1")
            .HasColumnType("varchar(250)")
            .IsRequired();

        builder.Property(ua => ua.AddressLine2)
            .HasColumnName("AddressLine2")
            .HasColumnType("varchar(250)");
           

        builder.Property(ua => ua.PinCode)
            .HasColumnName("PinCode")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(ua => ua.ContactNumber)
            .HasColumnName("ContactNumber")
            .HasColumnType("nvarchar(20)")
            .IsRequired();

        builder.Property(ua => ua.AlternateNumber)
            .HasColumnName("AlternateNumber")
            .HasColumnType("nvarchar(20)");
      

        builder.HasOne(ua => ua.Unit)
            .WithOne(u =>u.UnitAddress) 
           .HasForeignKey<UnitAddress>(ua => ua.UnitId);

   
        }
    }
}