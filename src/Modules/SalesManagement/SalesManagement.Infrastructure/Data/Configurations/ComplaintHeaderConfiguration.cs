using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintHeaderConfiguration : IEntityTypeConfiguration<ComplaintHeader>
    {
        public void Configure(EntityTypeBuilder<ComplaintHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v)
            );

            builder.ToTable("ComplaintHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintNumber).HasColumnName("ComplaintNumber").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.ComplaintDate).HasColumnName("ComplaintDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();
            builder.Property(t => t.CustomerId).HasColumnName("CustomerId").HasColumnType("int").IsRequired();

            // Customer Snapshot Fields
            builder.Property(t => t.CustomerAddress).HasColumnName("CustomerAddress").HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.CustomerPIN).HasColumnName("CustomerPIN").HasColumnType("varchar(10)").IsRequired(false);
            builder.Property(t => t.CustomerMobile).HasColumnName("CustomerMobile").HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(t => t.CustomerEmail).HasColumnName("CustomerEmail").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.CustomerPAN).HasColumnName("CustomerPAN").HasColumnType("varchar(10)").IsRequired(false);
            builder.Property(t => t.CustomerGSTNo).HasColumnName("CustomerGSTNo").HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(t => t.CreditLimit).HasColumnName("CreditLimit").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TotalOS).HasColumnName("TotalOS").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.Outstanding).HasColumnName("Outstanding").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.BalanceCredit).HasColumnName("BalanceCredit").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.Delay).HasColumnName("Delay").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.Ledger).HasColumnName("Ledger").HasColumnType("varchar(100)").IsRequired(false);

            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("varchar(500)").IsRequired(false);

            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK → MiscMaster (ComplaintStatus)
            builder.HasOne(t => t.Status)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection
            builder.HasMany(t => t.ComplaintDetails)
                .WithOne(d => d.ComplaintHeader)
                .HasForeignKey(d => d.ComplaintHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.ComplaintNumber).IsUnique();
            builder.HasIndex(t => t.CustomerId);
            builder.HasIndex(t => t.ComplaintDate);
            builder.HasIndex(t => t.StatusId);
        }
    }
}
