using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class TripSheetDetailConfiguration : IEntityTypeConfiguration<TripSheetDetail>
    {
        public void Configure(EntityTypeBuilder<TripSheetDetail> builder)
        {
            builder.ToTable("TripSheetDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TripSheetHeaderId)
                .HasColumnName("TripSheetHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchAdviceHeaderId)
                .HasColumnName("DispatchAdviceHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SequenceNo)
                .HasColumnName("SequenceNo")
                .HasColumnType("int")
                .IsRequired();

            // Same-module FK — DispatchAdviceHeader
            builder.HasOne(t => t.DispatchAdviceHeader)
                .WithMany()
                .HasForeignKey(t => t.DispatchAdviceHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.TripSheetHeaderId);
            builder.HasIndex(t => t.DispatchAdviceHeaderId);
            builder.HasIndex(t => new { t.TripSheetHeaderId, t.SequenceNo }).IsUnique();
            builder.HasIndex(t => new { t.TripSheetHeaderId, t.DispatchAdviceHeaderId }).IsUnique();
        }
    }
}
