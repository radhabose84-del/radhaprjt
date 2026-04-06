using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class LooseConeLedgerConfiguration : IEntityTypeConfiguration<LooseConeLedger>
    {
        public void Configure(EntityTypeBuilder<LooseConeLedger> builder)
        {
            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

            builder.ToTable("LooseConeLedger", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnName("LotId").HasColumnType("int").IsRequired();
            builder.Property(t => t.DocType).HasColumnName("DocType").HasColumnType("varchar(10)").IsRequired(false);
            builder.Property(t => t.DocNo).HasColumnName("DocNo").HasColumnType("int").IsRequired();
            builder.Property(t => t.DocDate).HasColumnName("DocDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();
            builder.Property(t => t.LooseConeIn).HasColumnName("LooseConeIn").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.LooseConeOut).HasColumnName("LooseConeOut").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.AsonLooseKgs).HasColumnName("AsonLooseKgs").HasColumnType("decimal(18,3)").IsRequired();

            // Index for running-balance queries (latest entry per unit+item+lot)
            builder.HasIndex(t => new { t.UnitId, t.ItemId, t.LotId, t.DocDate, t.Id });
        }
    }
}
