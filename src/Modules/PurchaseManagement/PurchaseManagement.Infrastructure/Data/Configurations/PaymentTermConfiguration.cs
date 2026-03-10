using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class PaymentTermConfiguration : IEntityTypeConfiguration<PaymentTermMaster>
    {
        public void Configure(EntityTypeBuilder<PaymentTermMaster> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            builder.ToTable("PaymentTermMaster", "Purchase");

            builder.HasKey(t => t.Id);
             builder.Property(x => x.Code)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.HasIndex(x => x.Code)
                   .IsUnique();

            builder.Property(x => x.Description)
                   .IsRequired()
                   .HasMaxLength(200);

            // Foreign key to MiscMaster (Baseline Date Type)
            builder.HasOne(x => x.BaselineType)
                   .WithMany(m => m.PaymentTermsAsBaselineType)               // no back-collection needed
                   .HasForeignKey(x => x.BaselineTypeId)
                   .HasPrincipalKey(m => m.Id)
                   .OnDelete(DeleteBehavior.Restrict);

            // Numbers / precision
            builder.Property(x => x.CreditDays)
                   .IsRequired();

            builder.Property(x => x.AdvancePercent)
                   .HasPrecision(5, 2);       // 0..100.00

            builder.Property(x => x.DiscountPercent)
                   .HasPrecision(5, 2);

            builder.Property(x => x.AdditionalValue)
                   .HasPrecision(18, 4)
                   .IsRequired();

            // Derived column: BalancePercent = 100 - ISNULL(AdvancePercent, 0)
            builder.Property(x => x.BalancePercent)
                   .HasPrecision(5, 2)
                   .HasComputedColumnSql(
                       "CONVERT(decimal(5,2), 100.00 - ISNULL([AdvancePercent], 0))",
                       stored: true);

            // Relationship to installments
            builder.HasMany(x => x.Installments)
                   .WithOne(i => i.PaymentTerm)
                   .HasForeignKey(i => i.PaymentTermId)
                   .OnDelete(DeleteBehavior.Cascade);


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

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");
       

            // CHECK constraints (SQL Server)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PaymentTermMaster_CreditDays", "[CreditDays] >= 0");
                t.HasCheckConstraint("CK_PaymentTermMaster_AdvancePercent", "[AdvancePercent] IS NULL OR ([AdvancePercent] >= 0 AND [AdvancePercent] <= 100)");
                t.HasCheckConstraint("CK_PaymentTermMaster_DiscountPercent", "[DiscountPercent] IS NULL OR ([DiscountPercent] >= 0 AND [DiscountPercent] <= 100)");
                t.HasCheckConstraint("CK_PaymentTermMaster_DiscountDays",
                    "[DiscountPercent] IS NULL OR ([DiscountDays] IS NOT NULL AND [DiscountDays] > 0)");
                t.HasCheckConstraint("CK_PaymentTermMaster_GraceDays",
                    "[GraceDays] IS NULL OR [GraceDays] >= 0");
            });

        }
    }
}