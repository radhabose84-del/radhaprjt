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

            // Indexes
            builder.HasIndex(t => t.GateInwardHdrId);

            // FK — cascade delete
            builder.HasOne(t => t.GateInwardHdr)
                .WithMany(h => h.GateInwardDetails)
                .HasForeignKey(t => t.GateInwardHdrId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
