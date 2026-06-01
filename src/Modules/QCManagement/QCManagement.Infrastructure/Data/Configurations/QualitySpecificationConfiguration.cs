using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QualitySpecificationConfiguration : IEntityTypeConfiguration<QualitySpecification>
    {
        public void Configure(EntityTypeBuilder<QualitySpecification> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QualitySpecification", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SpecificationCode)
                .HasColumnName("SpecificationCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.SpecificationName)
                .HasColumnName("SpecificationName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.QualityTemplateId)
                .HasColumnName("QualityTemplateId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ApplicableLevelId)
                .HasColumnName("ApplicableLevelId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QcTypeId)
                .HasColumnName("QcTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemCategoryId)
                .HasColumnName("ItemCategoryId")
                .HasColumnType("int");

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int");

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(500)");

            builder.Property(t => t.EffectiveFrom)
                .HasColumnName("EffectiveFrom")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(t => t.EffectiveTo)
                .HasColumnName("EffectiveTo")
                .HasColumnType("datetimeoffset");

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

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.SpecificationCode).IsUnique();
            builder.HasIndex(t => t.SpecificationName);
            builder.HasIndex(t => t.QualityTemplateId);
            builder.HasIndex(t => t.ApplicableLevelId);
            builder.HasIndex(t => t.QcTypeId);
            builder.HasIndex(t => t.ItemCategoryId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.EffectiveFrom);

            // FK: QualitySpecification → QualityTemplate (same module)
            builder.HasOne(t => t.QualityTemplate)
                .WithMany(d => d!.QualitySpecifications)
                .HasForeignKey(t => t.QualityTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecification → MiscMaster (ApplicableLevel, same module)
            builder.HasOne(t => t.ApplicableLevel)
                .WithMany(m => m!.QualitySpecificationsAsApplicableLevel)
                .HasForeignKey(t => t.ApplicableLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecification → MiscMaster (QcType, same module)
            builder.HasOne(t => t.QcType)
                .WithMany(m => m!.QualitySpecificationsAsQcType)
                .HasForeignKey(t => t.QcTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: ItemCategoryId and ItemId reference Inventory module (cross-module) — no DB FK constraint per CLAUDE.md
        }
    }
}
