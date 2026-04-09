using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class AgentCommissionPaymentTermConfiguration : IEntityTypeConfiguration<AgentCommissionPaymentTerm>
    {
        public void Configure(EntityTypeBuilder<AgentCommissionPaymentTerm> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("AgentCommissionPaymentTerm", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentCommissionConfigId)
                .HasColumnName("AgentCommissionConfigId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PaymentTermId)
                .HasColumnName("PaymentTermId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Composite unique index: prevent duplicate PaymentTerm per AgentCommissionConfig
            builder.HasIndex(t => new { t.AgentCommissionConfigId, t.PaymentTermId }).IsUnique();
            builder.HasIndex(t => t.AgentCommissionConfigId);
            builder.HasIndex(t => t.PaymentTermId);

            // FK: AgentCommissionPaymentTerm → AgentCommissionConfig (parent)
            builder.HasOne(t => t.AgentCommissionConfig)
                .WithMany(a => a.AgentCommissionPaymentTerms)
                .HasForeignKey(t => t.AgentCommissionConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FK: PaymentTermId → PurchaseManagement.PaymentTermMaster
            // NO DB FK constraint — validated via IPaymentTermLookup at application layer
        }
    }
}
