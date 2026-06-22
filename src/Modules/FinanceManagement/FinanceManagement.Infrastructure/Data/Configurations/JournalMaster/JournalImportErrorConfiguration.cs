using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    // US-GL01-17 — lean child log (NOT a BaseEntity). Cascades with its parent batch.
    public class JournalImportErrorConfiguration : IEntityTypeConfiguration<JournalImportError>
    {
        public void Configure(EntityTypeBuilder<JournalImportError> builder)
        {
            builder.ToTable("JournalImportError", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ImportBatchId).HasColumnName("ImportBatchId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RowNo).HasColumnName("RowNo").HasColumnType("int").IsRequired();
            builder.Property(t => t.ColumnName).HasColumnName("ColumnName").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.Message).HasColumnName("Message").HasColumnType("varchar(500)").IsRequired();

            builder.HasIndex(t => t.ImportBatchId);

            builder.HasOne(t => t.ImportBatch)
                .WithMany(b => b.Errors)
                .HasForeignKey(t => t.ImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
