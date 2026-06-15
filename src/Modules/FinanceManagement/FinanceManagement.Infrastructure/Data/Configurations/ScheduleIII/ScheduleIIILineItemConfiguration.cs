using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class ScheduleIIILineItemConfiguration : IEntityTypeConfiguration<ScheduleIIILineItem>
    {
        public void Configure(EntityTypeBuilder<ScheduleIIILineItem> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ScheduleIIILineItem", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.StructureId)
                .HasColumnName("StructureId").HasColumnType("int").IsRequired();

            builder.Property(t => t.SectionId)
                .HasColumnName("SectionId").HasColumnType("int").IsRequired();

            builder.Property(t => t.ParentLineId)
                .HasColumnName("ParentLineId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.LineCode)
                .HasColumnName("LineCode").HasColumnType("varchar(20)").IsRequired(false);

            builder.Property(t => t.LineName)
                .HasColumnName("LineName").HasColumnType("varchar(200)").IsRequired();

            builder.Property(t => t.SubClassification)
                .HasColumnName("SubClassification").HasColumnType("varchar(120)").IsRequired(false);

            builder.Property(t => t.NoteReference)
                .HasColumnName("NoteReference").HasColumnType("varchar(30)").IsRequired(false);

            builder.Property(t => t.DisplayOrder)
                .HasColumnName("DisplayOrder").HasColumnType("int")
                .HasDefaultValue(0).IsRequired();

            builder.Property(t => t.IsSplitLine)
                .HasColumnName("IsSplitLine").HasColumnType("bit")
                .HasDefaultValue(false).IsRequired();

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

            builder.HasOne(t => t.Structure)
                .WithMany(s => s.LineItems)
                .HasForeignKey(t => t.StructureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Section)
                .WithMany(s => s.LineItems)
                .HasForeignKey(t => t.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing parent/child (PPE > Tangible, Trade Payables > MSME ...)
            builder.HasOne(t => t.ParentLine)
                .WithMany(p => p.ChildLines)
                .HasForeignKey(t => t.ParentLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.StructureId);
            builder.HasIndex(t => t.SectionId);
            builder.HasIndex(t => t.ParentLineId);
        }
    }
}
