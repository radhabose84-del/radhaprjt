using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesAgreementHeaderConfiguration : IEntityTypeConfiguration<SalesAgreementHeader>
    {
        public void Configure(EntityTypeBuilder<SalesAgreementHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesAgreementHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgreementNo)
                .HasColumnName("AgreementNo")
                .HasColumnType("varchar(30)")
                .IsRequired(false);

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ValidFrom)
                .HasColumnName("ValidFrom")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ValidTo)
                .HasColumnName("ValidTo")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.CustomerId)
                .HasColumnName("CustomerId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesGroupId)
                .HasColumnName("SalesGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PaymentTermsId)
                .HasColumnName("PaymentTermsId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

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

            // Same-module FK constraints
            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesAgreementHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SalesGroup)
                .WithMany(g => g.SalesAgreementHeaders)
                .HasForeignKey(t => t.SalesGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.AgreementNo)
                .IsUnique()
                .HasFilter("[AgreementNo] IS NOT NULL");
            builder.HasIndex(t => t.CustomerId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.SalesGroupId);
            builder.HasIndex(t => new { t.ValidFrom, t.ValidTo });
        }
    }
}
