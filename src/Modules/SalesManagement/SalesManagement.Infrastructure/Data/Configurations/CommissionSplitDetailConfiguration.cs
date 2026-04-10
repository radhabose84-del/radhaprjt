using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class CommissionSplitDetailConfiguration : IEntityTypeConfiguration<CommissionSplitDetail>
    {
        public void Configure(EntityTypeBuilder<CommissionSplitDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CommissionSplitDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CommissionSplitId)
                .HasColumnName("CommissionSplitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RoleId)
                .HasColumnName("RoleId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ShareTypeId)
                .HasColumnName("ShareTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ShareValue)
                .HasColumnName("ShareValue")
                .HasColumnType("decimal(18,4)")
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
            builder.HasIndex(t => t.CommissionSplitId);
            builder.HasIndex(t => t.RoleId);
            builder.HasIndex(t => t.ShareTypeId);
            builder.HasIndex(t => new { t.CommissionSplitId, t.RoleId }).IsUnique();

            // FK: CommissionSplitDetail → CommissionSplit (parent)
            builder.HasOne(t => t.CommissionSplit)
                .WithMany(d => d.CommissionSplitDetails)
                .HasForeignKey(t => t.CommissionSplitId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: CommissionSplitDetail → MiscMaster (Role)
            builder.HasOne(t => t.Role)
                .WithMany(m => m.CommissionSplitDetailsAsRole)
                .HasForeignKey(t => t.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: CommissionSplitDetail → MiscMaster (ShareType)
            builder.HasOne(t => t.ShareType)
                .WithMany(m => m.CommissionSplitDetailsAsShareType)
                .HasForeignKey(t => t.ShareTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
