using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QCManagement.Domain.Entities;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> b)
        {
            b.ToTable("ActivityLog", "QC");
            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedDate).HasColumnType("datetimeoffset").IsRequired();
            b.Property(x => x.EntityName).HasColumnType("varchar(60)").IsRequired();
            b.Property(x => x.EntityId).HasColumnType("int").IsRequired();
            b.Property(x => x.Action).HasColumnType("varchar(40)").IsRequired();
            b.Property(x => x.PropertyName).HasColumnType("varchar(200)").IsRequired();
            b.Property(x => x.OldValue).HasColumnType("varchar(max)");
            b.Property(x => x.NewValue).HasColumnType("varchar(max)");
            b.Property(x => x.CreatedBy).HasColumnType("int");
            b.Property(x => x.CreatedByName).HasColumnType("varchar(100)");
            b.Property(x => x.CreatedIP).HasColumnType("varchar(50)");
            b.Property(x => x.Scope).HasColumnType("varchar(40)");
            b.Property(x => x.ScopeKey).HasColumnType("varchar(120)");

            b.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedDate })
             .HasDatabaseName("IX_ActivityLog_Entity_CreatedDate");
        }
    }
}
