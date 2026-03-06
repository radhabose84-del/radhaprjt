using BackgroundService.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Persistence.Configurations
{
    public sealed class NotificationTablePresetConfiguration : IEntityTypeConfiguration<NotificationTablePreset>
    {
        public void Configure(EntityTypeBuilder<NotificationTablePreset> b)
        {
            b.ToTable("NotificationTablePreset", schema: "Notification");

            b.HasKey(x => x.Id);

            b.Property(x => x.PresetKey)
                .IsRequired()
                .HasMaxLength(64);

            b.HasIndex(x => x.PresetKey)
                .IsUnique()
                .HasDatabaseName("UX_TablePresets_PresetKey");

            b.Property(x => x.TemplateId).IsRequired();
            b.HasIndex(x => x.TemplateId);

            b.HasOne(x => x.Template)
            .WithMany()              
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.ColumnsJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            

            // Validate JSON at DB level (SQL Server)
            b.ToTable(tb =>
            {
                tb.HasCheckConstraint(
                    "CK_TablePresets_ColumnsJson_IsJson",
                    "ISJSON([ColumnsJson]) = 1"
                );
            });

            b.Property(x => x.Version)
                .HasColumnType("int")
                .IsRequired(false);

            b.Property(x => x.IsActive)
                .HasDefaultValue(true);

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()"); 

            // rowversion for optimistic concurrency
            b.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
