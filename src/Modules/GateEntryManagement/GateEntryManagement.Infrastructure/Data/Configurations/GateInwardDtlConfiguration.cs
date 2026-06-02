using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GateEntryManagement.Domain.Entities;

namespace GateEntryManagement.Infrastructure.Data.Configurations
{
    public class GateInwardDtlConfiguration : IEntityTypeConfiguration<GateInwardDtl>
    {
        public void Configure(EntityTypeBuilder<GateInwardDtl> builder)
        {
            builder.ToTable("GateInwardDtl", "Gate");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.GateInwardHdrId)
                .HasColumnName("GateInwardHdrId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReferenceDocTypeId)
                .HasColumnName("ReferenceDocTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReferenceDocNo)
                .HasColumnName("ReferenceDocNo")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.PartyName)
                .HasColumnName("PartyName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            // Minimum PO context — only fields that aren't derivable from Purchase / Inventory.
            // All three are nullable so the same table can hold non-PO (manual) lines too.
            builder.Property(t => t.PoId)
                .HasColumnName("PoId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.PoSlNoLocal)
                .HasColumnName("PoSlNoLocal")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.DcQuantity)
                .HasColumnName("DcQuantity")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            // Optional supplier-supplied expiry — passed straight through to Purchase.GrnDetail.
            builder.Property(t => t.ExpiryDate)
                .HasColumnName("ExpiryDate")
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // Indexes
            builder.HasIndex(t => t.GateInwardHdrId);
            builder.HasIndex(t => t.PoId);

            // FK — cascade delete
            builder.HasOne(t => t.GateInwardHdr)
                .WithMany(h => h.GateInwardDetails)
                .HasForeignKey(t => t.GateInwardHdrId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
