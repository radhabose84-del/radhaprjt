using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ProformaInvoiceConfiguration : IEntityTypeConfiguration<ProformaInvoice>
    {
        public void Configure(EntityTypeBuilder<ProformaInvoice> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ProformaInvoice", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProformaNumber)
                .HasColumnName("ProformaNumber")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.ProformaDate)
                .HasColumnName("ProformaDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.SalesOrderId)
                .HasColumnName("SalesOrderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProformaAmount)
                .HasColumnName("ProformaAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.SOBalance)
                .HasColumnName("SOBalance")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.PaymentReceivedAmount)
                .HasColumnName("PaymentReceivedAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            // Status & Audit
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK constraints
            builder.HasOne(t => t.SalesOrderHeader)
                .WithMany(h => h.ProformaInvoices)
                .HasForeignKey(t => t.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.ProformaInvoicesAsStatus)
                .HasForeignKey(t => t.StatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.ProformaNumber).IsUnique();
            builder.HasIndex(t => t.SalesOrderId);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.ProformaDate);
        }
    }
}
