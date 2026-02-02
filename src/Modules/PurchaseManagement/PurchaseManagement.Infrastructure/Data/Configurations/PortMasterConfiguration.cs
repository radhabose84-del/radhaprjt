using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations;
public sealed class PortMasterConfiguration : IEntityTypeConfiguration<PortMaster>
{
    public void Configure(EntityTypeBuilder<PortMaster> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
           v => v == Status.Active,                    // Convert to DB (1 for Active)
           v => v ? Status.Active : Status.Inactive    // Convert to Entity
       );

        // ValueConverter for IsDelete (enum to bit)
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
        );
        b.ToTable("PortMaster", "Purchase");
        b.HasKey(x => x.Id);
        b.Property(x => x.PortCode).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(x => x.PortName).HasMaxLength(150).IsRequired();

        b.Property(x => x.PortTypeId).IsRequired(false);
        b.HasIndex(x => x.PortCode).IsUnique().HasFilter("[IsDeleted] = 0");

        b.HasOne(ac => ac.MiscType)
         .WithMany(am => am.PortType)
         .HasForeignKey(ac => ac.TypeId)
         .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(ac => ac.MiscPortType)
             .WithMany(am => am.PortMiscType)
             .HasForeignKey(ac => ac.PortTypeId)
             .OnDelete(DeleteBehavior.NoAction);
            
           
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
    }
}
