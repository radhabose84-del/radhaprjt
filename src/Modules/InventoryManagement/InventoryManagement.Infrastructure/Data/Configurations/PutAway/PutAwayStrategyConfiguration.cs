using InventoryManagement.Domain.Entities.Item.PutAway;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Data.Configurations
{
    public sealed class PutAwayStrategyConfiguration : IEntityTypeConfiguration<PutAwayStrategy>
    {
        public void Configure(EntityTypeBuilder<PutAwayStrategy> b)
        {
            // enum -> bit converters
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            b.ToTable("PutAwayStrategy", "Inventory");
            b.HasKey(x => x.Id);

            b.Property(x => x.TargetId).IsRequired(false);

            // Strategy → Rule
            b.HasOne(x => x.PutAwayRule)
             .WithMany(r => r.Strategies)
             .HasForeignKey(x => x.PutAwayRuleId)
             .OnDelete(DeleteBehavior.Cascade);

            // StorageType (FK: StorageTypeId) → MiscMaster (inverse: PutAwayStrategyStorageType)
            b.HasOne(x => x.MiscStorageTypeId)
             .WithMany(m => m.PutAwayStrategyStorageType)
             .HasForeignKey(x => x.StorageTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            // Priority (FK: PriorityId) → MiscMaster (inverse: PutAwayStrategyPriority)
            b.HasOne(x => x.MiscPriority)
             .WithMany(m => m.PutAwayStrategyPriority)
             .HasForeignKey(x => x.PriorityId)
             .OnDelete(DeleteBehavior.Restrict);

            // Unique priority per rule
            b.HasIndex(x => new { x.PutAwayRuleId, x.PriorityId })
             .IsUnique()
             .HasDatabaseName("UX_PutAwayStrategy_PriorityPerRule")
             .HasFilter("[IsDeleted] = 0"); 

            // audit/status
            b.Property(x => x.IsActive).HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            b.Property(x => x.IsDeleted).HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            b.Property(x => x.CreatedBy).HasColumnType("int").IsRequired();
            b.Property(x => x.CreatedDate).HasColumnType("datetimeoffset").IsRequired(false);
            b.Property(x => x.CreatedByName).HasColumnType("varchar(50)").IsRequired(false);
            b.Property(x => x.CreatedIP).HasColumnType("varchar(50)").IsRequired(false);
            b.Property(x => x.ModifiedBy).HasColumnType("int").IsRequired(false);
            b.Property(x => x.ModifiedDate).HasColumnType("datetimeoffset").IsRequired(false);
            b.Property(x => x.ModifiedByName).HasColumnType("varchar(50)").IsRequired(false);
            b.Property(x => x.ModifiedIP).HasColumnType("varchar(50)").IsRequired(false);
        }
    }
}
