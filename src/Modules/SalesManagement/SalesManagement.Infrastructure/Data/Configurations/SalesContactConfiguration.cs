using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesContactConfiguration : IEntityTypeConfiguration<SalesContact>
    {
        public void Configure(EntityTypeBuilder<SalesContact> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesContact", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ContactName)
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.MobileNumber)
                .HasColumnType("varchar(15)")
                .IsRequired();

            builder.Property(t => t.ContactTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Email)
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.ContactName)
                .HasDatabaseName("IX_SalesContact_ContactName");

            builder.HasIndex(t => t.MobileNumber)
                .HasDatabaseName("IX_SalesContact_MobileNumber");

            builder.HasIndex(t => t.ContactTypeId)
                .HasDatabaseName("IX_SalesContact_ContactTypeId");

            builder.HasIndex(t => t.PartyId)
                .HasDatabaseName("IX_SalesContact_PartyId");

            // Same-module FK: ContactTypeId → Sales.MiscMaster
            builder.HasOne(t => t.ContactType)
                .WithMany(m => m.SalesContacts)
                .HasForeignKey(t => t.ContactTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // No FK constraint for PartyId (cross-module — PartyManagement)
        }
    }
}
