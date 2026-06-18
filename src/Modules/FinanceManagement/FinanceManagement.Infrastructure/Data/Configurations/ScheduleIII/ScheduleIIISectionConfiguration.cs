using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class ScheduleIIISectionConfiguration : IEntityTypeConfiguration<ScheduleIIISection>
    {
        public void Configure(EntityTypeBuilder<ScheduleIIISection> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ScheduleIIISection", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.SectionName)
                .HasColumnName("SectionName").HasColumnType("varchar(150)").IsRequired();

            // Same-module FK -> Finance.MiscMaster (S3_STMT_TYPE)
            builder.Property(t => t.StatementTypeId)
                .HasColumnName("StatementTypeId").HasColumnType("int").IsRequired();

            // Same-module FK -> Finance.MiscMaster (S3_NATURE)
            builder.Property(t => t.NatureId)
                .HasColumnName("NatureId").HasColumnType("int").IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit")
                .HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasOne(t => t.StatementType)
                .WithMany()
                .HasForeignKey(t => t.StatementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Nature)
                .WithMany()
                .HasForeignKey(t => t.NatureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.StatementTypeId);
            builder.HasIndex(t => t.NatureId);
        }
    }
}
