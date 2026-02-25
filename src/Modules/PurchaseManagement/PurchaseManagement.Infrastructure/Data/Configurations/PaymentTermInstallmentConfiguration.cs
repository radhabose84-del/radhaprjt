using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class PaymentTermInstallmentConfiguration : IEntityTypeConfiguration<PaymentTermInstallment>
    {
        public void Configure(EntityTypeBuilder<PaymentTermInstallment> builder)
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
            builder.ToTable("PaymentTermInstallment", "Purchase");

             builder.HasKey(i => i.Id);

            builder.Property(i => i.Percent)
                   .HasPrecision(5,2);

            // FK already defined from the parent config, but defining here is fine too
            builder.HasOne(i => i.PaymentTerm)
                   .WithMany(t => t.Installments)
                   .HasForeignKey(i => i.PaymentTermId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One sequence number per term
            builder.HasIndex(i => new { i.PaymentTermId, i.SeqNo }).IsUnique();

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

            // Safety rails
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PaymentTermInstallment_SeqNo", "[SeqNo] >= 1");
                t.HasCheckConstraint("CK_PaymentTermInstallment_Percent", "[Percent] >= 0 AND [Percent] <= 100");
                t.HasCheckConstraint("CK_PaymentTermInstallment_DueDays", "[DueDays] >= 0");
            });

        }    
    }
}