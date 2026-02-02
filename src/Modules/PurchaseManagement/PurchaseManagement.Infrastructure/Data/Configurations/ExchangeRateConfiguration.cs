
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations;

public  class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{    
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRate", "Purchase");
        builder.Property(x => x.BaseCurrency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.QuoteCurrency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Rate).HasColumnType("decimal(18,8)").IsRequired();
        builder.Property(x => x.Source).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ActualRate).HasColumnType("decimal(18,8)").IsRequired(false);
        builder.HasIndex(x => new { x.BaseCurrency, x.QuoteCurrency, x.EffectiveDate })
             .IsUnique()
             .HasDatabaseName("UX_Rate_Base_Quote_Date");
    }
}
