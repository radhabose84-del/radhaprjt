using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.Infrastructure.Data.Configurations
{
    public class GatePassDtlConfiguration : IEntityTypeConfiguration<GatePassDtl>
    {
        public void Configure(EntityTypeBuilder<GatePassDtl> builder)
        {
            builder.ToTable("GatePassDtl", "Gate");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GatePassHdrId)
                .HasColumnName("GatePassHdrId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocTypeId)
                .HasColumnName("DocTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocId)
                .HasColumnName("DocId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DocNo)
                .HasColumnName("DocNo")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.PartyName)
                .HasColumnName("PartyName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.PartyCode)
                .HasColumnName("PartyCode")
                .HasColumnType("varchar(20)")
                .IsRequired(false);

            builder.Property(t => t.DocDate)
                .HasColumnName("DocDate")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(t => t.TotalQty)
                .HasColumnName("TotalQty")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.GatePassHdrId);

            // FK to header — cascade delete
            builder.HasOne(t => t.GatePassHdr)
                .WithMany(h => h.GatePassDetails)
                .HasForeignKey(t => t.GatePassHdrId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
