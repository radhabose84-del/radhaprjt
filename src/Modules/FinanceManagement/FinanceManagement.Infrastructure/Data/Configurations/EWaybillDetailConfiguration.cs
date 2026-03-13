using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class EWaybillDetailConfiguration : IEntityTypeConfiguration<EWaybillDetail>
    {
        public void Configure(EntityTypeBuilder<EWaybillDetail> builder)
        {
            builder.ToTable("EWaybillDetail", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnType("int").IsRequired();
            builder.Property(t => t.EWaybillHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemSno).HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemName).HasColumnType("varchar(200)").IsRequired(false);
            builder.Property(t => t.HsnNo).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.IsService).HasColumnType("char(1)").HasDefaultValue("N").IsRequired(false);
            builder.Property(t => t.Qty).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.UOM).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.TaxableValue).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxRate).HasColumnType("decimal(5,2)").IsRequired();
            builder.Property(t => t.CGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Cess).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IsActive).HasColumnType("bit").HasDefaultValue(true).IsRequired();
            builder.Property(t => t.IsDeleted).HasColumnType("bit").HasDefaultValue(false).IsRequired();

            // Indexes
            builder.HasIndex(t => t.EWaybillHeaderId);
            builder.HasIndex(t => t.ItemId);
        }
    }
}
