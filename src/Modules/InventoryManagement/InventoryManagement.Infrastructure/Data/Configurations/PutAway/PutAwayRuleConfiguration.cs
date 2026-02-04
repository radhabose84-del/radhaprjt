// PutAwayRuleConfiguration.cs
using InventoryManagement.Domain.Entities.Item.PutAway;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

public sealed class PutAwayRuleConfiguration : IEntityTypeConfiguration<PutAwayRule>
{
    public void Configure(EntityTypeBuilder<PutAwayRule> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );
        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        b.ToTable("PutAwayRule", "Inventory");
        b.HasKey(x => x.Id);

        b.Property(x => x.UnitId).IsRequired();
        b.Property(x => x.ItemGroupId).IsRequired();
        b.Property(x => x.ItemCategoryId).IsRequired();
        b.Property(x => x.WarehouseId).IsRequired();

        // Pair BOTH sides to avoid ...Id1
        b.HasOne(x => x.ItemGroup)
         .WithMany(g => g.PutAwayRuleGroup)
         .HasForeignKey(x => x.ItemGroupId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ItemCategory)
         .WithMany(c => c.PutAwayRuleCategory)
         .HasForeignKey(x => x.ItemCategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        // ✅ Explicitly pair the inverse to avoid a second relationship
        b.HasOne(x => x.ItemMaster)
         .WithMany(m => m.PutAwayRules)   // <— IMPORTANT
         .HasForeignKey(x => x.ItemId)    // <— uses ItemId as the ONLY FK
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired(false);

        b.HasMany(x => x.Strategies)
         .WithOne(s => s.PutAwayRule)
         .HasForeignKey(s => s.PutAwayRuleId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.UnitId, x.WarehouseId, x.ItemGroupId, x.ItemCategoryId, x.ItemId })
         .IsUnique()
         .HasDatabaseName("UX_PutAwayRule_Scope");

        b.Property(x => x.IsActive).HasColumnType("bit").HasConversion(statusConverter);
        b.Property(x => x.IsDeleted).HasColumnType("bit").HasConversion(isDeleteConverter);
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
