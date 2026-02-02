using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.Purchase
{
    public class DutyMasterConfiguration : IEntityTypeConfiguration<DutyMaster>
    {
        public void Configure(EntityTypeBuilder<DutyMaster> b)
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

            b.ToTable("DutyMaster", "Purchase");

            b.HasKey(x => x.Id);

            b.Property(x => x.DutyCode)
                .IsRequired()
                .HasMaxLength(50);
                

            b.Property(x => x.TariffNumber)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(x => x.HsnCode)
                .HasMaxLength(50);

            b.Property(x => x.NotificationNumber)
                .HasMaxLength(50);

            b.Property(x => x.Remarks)
                .HasMaxLength(500);
             b.Property(x => x.Description)
                .HasMaxLength(500);

            // decimals (5,2)
            b.Property(x => x.BasicCustomsDutyPercentage).HasPrecision(5, 2);
            b.Property(x => x.SocialWelfareSurchargePercentage).HasPrecision(5, 2);
            b.Property(x => x.IGSTPercentage).HasPrecision(5, 2);
            b.Property(x => x.AgriInfraDevCessPercentage).HasPrecision(5, 2);
            b.Property(x => x.AntiDumpingDutyPercentage).HasPrecision(5, 2);
            b.Property(x => x.SafeguardDutyPercentage).HasPrecision(5, 2);
            b.Property(x => x.HealthEducationCessPercentage).HasPrecision(5, 2);

            b.Property(x => x.EffectiveFrom).IsRequired();
            b.HasOne(x => x.MiscDuty)
                .WithMany(m => m.dutyCategory)
                .HasForeignKey(x => x.DutyCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.MiscCOA)
                .WithMany(m => m.dutyCOA)
                .HasForeignKey(x => x.CountryOfOriginApplicability)
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
}
