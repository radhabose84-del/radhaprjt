using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintDetailNatureConfiguration : IEntityTypeConfiguration<ComplaintDetailNature>
    {
        public void Configure(EntityTypeBuilder<ComplaintDetailNature> builder)
        {
            builder.ToTable("ComplaintDetailNature", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintDetailId).HasColumnName("ComplaintDetailId").HasColumnType("int").IsRequired();
            builder.Property(t => t.NatureOfComplaintId).HasColumnName("NatureOfComplaintId").HasColumnType("int").IsRequired();

            // Same-module FK → MiscMaster (NatureOfComplaint)
            builder.HasOne(t => t.NatureOfComplaintMisc)
                .WithMany()
                .HasForeignKey(t => t.NatureOfComplaintId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.ComplaintDetailId);
            builder.HasIndex(t => t.NatureOfComplaintId);
        }
    }
}
