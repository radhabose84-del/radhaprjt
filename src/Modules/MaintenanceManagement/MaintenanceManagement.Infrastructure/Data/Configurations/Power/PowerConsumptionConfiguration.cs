using MaintenanceManagement.Domain.Entities.Power;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.Power
{
    public class PowerConsumptionConfiguration : IEntityTypeConfiguration<PowerConsumption>
    {
        public void Configure(EntityTypeBuilder<PowerConsumption> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                 v => v == Status.Active,
                 v => v ? Status.Active : Status.Inactive
             );
            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("PowerConsumption", "Maintenance");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
            
            builder.Property(t => t.FeederTypeId)
                .HasColumnName("FeederTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(f => f.FeederTypePower)
                .WithMany(m => m.FeedersPower)
                .HasForeignKey(f => f.FeederTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.FeederId)
                .HasColumnName("FeederId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(f => f.FeederPower)
                .WithMany(m => m.FeederConsumptions)
                .HasForeignKey(f => f.FeederId)
                .OnDelete(DeleteBehavior.Restrict);

             builder.Property(ag => ag.UnitId)
               .HasColumnName("UnitId")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(t => t.OpeningReading)
                .HasColumnName("OpeningReading")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ClosingReading)
                .HasColumnName("ClosingReading")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalUnits)
                .HasColumnName("TotalUnits")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();
            

             builder.Property(t => t.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();


            builder.Property(t => t.IsDeleted)
            .HasColumnType("bit")
            .IsRequired()
            .HasConversion(isDeleteConverter);
            
             builder.Property(t => t.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(max)");

            builder.Property(t => t.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(t => t.ModifiedByName)
                .HasColumnType("varchar(max)");

            builder.Property(t => t.ModifiedIP)
                .HasColumnType("varchar(50)");  
        }
            
    }
}