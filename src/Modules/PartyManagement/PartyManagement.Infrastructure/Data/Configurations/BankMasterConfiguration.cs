using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Data.Configurations;

public class BankMasterConfiguration : IEntityTypeConfiguration<BankMaster>
{
    public void Configure(EntityTypeBuilder<BankMaster> b)
    {
        // ValueConverter for Status (enum to bit)
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        // ValueConverter for IsDelete (enum to bit)
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("BankMaster", schema: "Party");
        b.HasKey(x => x.Id);

        b.Property(x => x.BankCode).IsRequired().HasMaxLength(20);
        b.Property(x => x.BankName).IsRequired().HasMaxLength(100);

        b.HasIndex(x => x.BankCode).IsUnique();

        b.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");


        b.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

        b.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

        b.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");

        b.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

        b.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

    }
}