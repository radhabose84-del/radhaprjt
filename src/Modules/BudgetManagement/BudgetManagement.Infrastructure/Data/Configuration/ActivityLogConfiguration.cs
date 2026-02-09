using BudgetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetManagement.Infrastructure.Data.Configurations;
public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> b)
    {
        b.ToTable("ActivityLog", "Budget"); // or "Common"
        b.HasKey(x => x.Id);

        b.Property(x => x.EntityName).HasMaxLength(60).IsRequired();
        b.Property(x => x.EntityId).IsRequired();
        b.Property(x => x.Action).HasMaxLength(40).IsRequired();
        b.Property(x => x.PropertyName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Scope).HasMaxLength(40);
        b.Property(x => x.ScopeKey).HasMaxLength(120);

        b.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedDate })
         .HasDatabaseName("IX_ActivityLog_Entity_CreatedDate");
    }
}
