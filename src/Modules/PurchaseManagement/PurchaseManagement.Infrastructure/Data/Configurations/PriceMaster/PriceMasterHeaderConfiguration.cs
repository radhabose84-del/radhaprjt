using PurchaseManagement.Domain.Entities.PriceMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.PriceMaster
{
    public sealed class PriceMasterHeaderConfiguration : IEntityTypeConfiguration<PriceMasterHeader>
    {
        public void Configure(EntityTypeBuilder<PriceMasterHeader> b)
        {            
            b.ToTable("PriceMasterHeader", "Purchase");
            b.HasKey(x => x.Id);

            b.Property(x => x.UnitId).IsRequired();
            b.Property(x => x.ItemId).IsRequired();
            b.Property(x => x.VendorId).IsRequired();

            b.Property(p => p.ValidFrom).HasColumnType("date");
            b.Property(p => p.ValidTo).HasColumnType("date");
         
            b.Property(x => x.StatusId).IsRequired();
            b.Property(x => x.SourceFromId).IsRequired();
            b.Property(x => x.SourceDetailId).IsRequired(false);
            b.Property(x => x.UomId).IsRequired();

            // one → many (Header → Details)
            b.HasOne(ac => ac.MiscStatus)
             .WithMany(am => am.PriceMasterStatus)
             .HasForeignKey(ac => ac.StatusId)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasMany(x => x.Details)
                .WithOne(d => d.PriceMasterHeader)
                .HasForeignKey(d => d.PriceMasterHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK to MiscMaster for SourceFrom
            b.HasOne(x => x.MiscSourceFrom)
                .WithMany(am => am.PriceMasterSourceFrom)
                .HasForeignKey(x => x.SourceFromId)
                .OnDelete(DeleteBehavior.NoAction);

            // Helpful indexes
            
            b.HasIndex(x => new { x.ItemId, x.VendorId, x.ValidFrom ,x.IsActive });
            b.HasIndex(x => new { x.ItemId, x.VendorId, x.ValidTo, x.IsActive });
        }
    }
}
