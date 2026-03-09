using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
    {
        public void Configure(EntityTypeBuilder<DocumentSequence> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DocumentSequence", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TypeId)
                .HasColumnName("TypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FinancialYearId)
                .HasColumnName("FinancialYearId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocNo)
                .HasColumnName("DocNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Composite unique index — no duplicate TypeId + FinancialYearId + DocNo combinations
            builder.HasIndex(t => new { t.TypeId, t.FinancialYearId, t.DocNo }).IsUnique();

            // Individual indexes for filtering
            builder.HasIndex(t => t.TypeId);
            builder.HasIndex(t => t.FinancialYearId);

            // Same-module FK constraint (Finance.TransactionTypeMaster)
            builder.HasOne(t => t.TransactionTypeMaster)
                .WithMany(ttm => ttm.DocumentSequences)
                .HasForeignKey(t => t.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
