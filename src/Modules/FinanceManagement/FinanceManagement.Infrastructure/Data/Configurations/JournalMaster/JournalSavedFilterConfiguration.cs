using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-14 — per-user saved filters (NOT a BaseEntity).
    public class JournalSavedFilterConfiguration : IEntityTypeConfiguration<JournalSavedFilter>
    {
        public void Configure(EntityTypeBuilder<JournalSavedFilter> builder)
        {
            builder.ToTable("JournalSavedFilter", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.UserId).HasColumnName("UserId").HasColumnType("int").IsRequired();
            builder.Property(t => t.Name).HasColumnName("Name").HasColumnType("varchar(100)").IsRequired();
            builder.Property(t => t.CriteriaJson).HasColumnName("CriteriaJson").HasColumnType("nvarchar(max)").IsRequired();

            builder.HasIndex(t => new { t.UserId, t.Name })
                .IsUnique()
                .HasDatabaseName("UX_JournalSavedFilter_UserName");

            builder.HasIndex(t => t.UserId);

            // UserId is a cross-module reference — no DB FK constraint.
        }
    }
}
