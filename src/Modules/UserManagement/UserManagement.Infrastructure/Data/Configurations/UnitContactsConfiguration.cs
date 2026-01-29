using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UnitContactsConfiguration : IEntityTypeConfiguration<UnitContacts>
    {
        public void Configure(EntityTypeBuilder<UnitContacts> builder)
        {
              builder.ToTable("UnitContacts", "AppData");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(uc => uc.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(uc => uc.Name)
            .HasColumnName("Name")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(uc => uc.Designation)
            .HasColumnName("Designation")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(uc => uc.Email)
            .HasColumnName("Email")
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder.Property(uc => uc.PhoneNo)
            .HasColumnName("PhoneNo")
            .HasColumnType("nvarchar(20)")
            .IsRequired();

        builder.Property(uc => uc.Remarks)
            .HasColumnName("Remarks")
            .HasColumnType("varchar(250)");

        builder.HasOne(uc => uc.Unit)
            .WithOne(u => u.UnitContacts)
            .HasForeignKey<UnitContacts>(uc => uc.UnitId);
        }
    }
}