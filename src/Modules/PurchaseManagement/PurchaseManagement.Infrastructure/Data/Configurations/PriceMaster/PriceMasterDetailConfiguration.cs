using PurchaseManagement.Domain.Entities.PriceMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.PriceMaster
{
    public sealed class PriceMasterDetailConfiguration : IEntityTypeConfiguration<PriceMasterDetail>
    {
        public void Configure(EntityTypeBuilder<PriceMasterDetail> b)
        {            
            b.ToTable("PriceMasterDetail", "Purchase");
            b.HasKey(x => x.Id);

            b.Property(x => x.ScaleQtyFrom).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.ScaleQtyTo).HasPrecision(18, 2);
            b.Property(x => x.UnitPrice).HasPrecision(18, 4).IsRequired();
            b.Property(x => x.CurrencyId).IsRequired();

            b.HasIndex(x => new { x.PriceMasterHeaderId, x.ScaleQtyFrom });
        }
    }
}
