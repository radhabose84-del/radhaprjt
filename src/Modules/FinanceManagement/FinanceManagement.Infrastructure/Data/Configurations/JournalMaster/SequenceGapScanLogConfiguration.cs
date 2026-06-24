using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-03B — lean job log (NOT a BaseEntity).
    public class SequenceGapScanLogConfiguration : IEntityTypeConfiguration<SequenceGapScanLog>
    {
        public void Configure(EntityTypeBuilder<SequenceGapScanLog> builder)
        {
            builder.ToTable("SequenceGapScanLog", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.SeriesId).HasColumnName("SeriesId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ScannedAt).HasColumnName("ScannedAt").IsRequired();
            builder.Property(t => t.GapsFound).HasColumnName("GapsFound").HasColumnType("int").HasDefaultValue(0).IsRequired();
            builder.Property(t => t.GapNumbers).HasColumnName("GapNumbers").HasColumnType("varchar(max)").IsRequired(false);
            builder.Property(t => t.Alerted).HasColumnName("Alerted").HasColumnType("bit").HasDefaultValue(false).IsRequired();

            builder.HasIndex(t => t.SeriesId);
            builder.HasIndex(t => t.ScannedAt);

            builder.HasOne(t => t.Series)
                .WithMany()
                .HasForeignKey(t => t.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
