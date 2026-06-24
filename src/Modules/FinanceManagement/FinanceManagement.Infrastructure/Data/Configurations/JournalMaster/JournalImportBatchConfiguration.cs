using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    public class JournalImportBatchConfiguration : IEntityTypeConfiguration<JournalImportBatch>
    {
        public void Configure(EntityTypeBuilder<JournalImportBatch> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("JournalImportBatch", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.FileName).HasColumnName("FileName").HasColumnType("varchar(260)").IsRequired();
            builder.Property(t => t.TotalRows).HasColumnName("TotalRows").HasColumnType("int").HasDefaultValue(0).IsRequired();
            builder.Property(t => t.ValidRows).HasColumnName("ValidRows").HasColumnType("int").HasDefaultValue(0).IsRequired();
            builder.Property(t => t.ErrorRows).HasColumnName("ErrorRows").HasColumnType("int").HasDefaultValue(0).IsRequired();
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SourceId).HasColumnName("SourceId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ImportedBy).HasColumnName("ImportedBy").HasColumnType("int").IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.ImportedBy);

            builder.HasOne(t => t.BatchStatus)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Source)
                .WithMany()
                .HasForeignKey(t => t.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            // ImportedBy is a cross-module reference — no DB FK constraint.
        }
    }
}
