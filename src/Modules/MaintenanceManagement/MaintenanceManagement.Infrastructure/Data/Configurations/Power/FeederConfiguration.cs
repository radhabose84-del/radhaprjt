using MaintenanceManagement.Domain.Entities.Power;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.Power
{
    public class FeederConfiguration : IEntityTypeConfiguration<Feeder>
    {
        public void Configure(EntityTypeBuilder<Feeder> builder)
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

            builder.ToTable("Feeder", "Maintenance");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FeederCode)
                .HasColumnName("FeederCode")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.FeederName)
                .HasColumnName("FeederName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.HasOne(f => f.FeederGroup)
                .WithMany(g => g.Feeders)
                .HasForeignKey(f => f.FeederGroupId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(f => f.FeederType)
                .WithMany(m => m.Feeders)
                .HasForeignKey(f => f.FeederTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Property(t => t.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(t => t.MultiplicationFactor)
                .HasColumnName("MultiplicationFactor")
                .HasColumnType("Decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.EffectiveDate)
                .HasColumnName("EffectiveDate")
                .HasColumnType("DateTimeOffset")
                .IsRequired();

            builder.Property(t => t.OpeningReading)
                .HasColumnName("OpeningReading")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.HighPriority)
                .HasColumnName("HighPriority")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.Target)
                .HasColumnName("Target")
                .HasColumnType("Decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.MeterAvailable)
                .HasColumnName("MeterAvailable")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.MeterTypeId)
                .HasColumnName("MeterTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ParentFeederId)
            .HasColumnName("ParentFeederId")
            .HasColumnType("int")
                .IsRequired(false);

             builder.HasOne(f => f.ParentFeeder)
            .WithMany(f => f.SubFeeders)
            .HasForeignKey(f => f.ParentFeederId)
            .OnDelete(DeleteBehavior.Restrict);


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