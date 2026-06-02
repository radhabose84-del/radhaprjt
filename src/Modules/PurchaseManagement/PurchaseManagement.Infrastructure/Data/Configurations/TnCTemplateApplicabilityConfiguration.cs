using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class TnCTemplateApplicabilityConfiguration  : IEntityTypeConfiguration<TnCTemplateApplicability>
    {
        public void Configure(EntityTypeBuilder<TnCTemplateApplicability> builder)
        {

             var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("TnCTemplateApplicability", "Purchase");

            builder.HasKey(x => x.Id);

            // FK to master
            builder.Property(x => x.TnCTemplateMasterId)
                   .IsRequired();

            // Cross-module FK to Finance.TransactionTypeMaster — plain column, NO DB FK constraint / navigation
            builder.Property(x => x.TransactionTypeId)
                   .IsRequired();

            // Relations (same-module master only)
            builder.HasOne(x => x.TnCTemplate)
             .WithMany(t => t.Applicabilities)
             .HasForeignKey(x => x.TnCTemplateMasterId)
             .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate transaction type per template (respect soft-delete)
            builder.HasIndex(x => new { x.TnCTemplateMasterId, x.TransactionTypeId })
             .IsUnique()
             .HasDatabaseName("UX_TnC_Template_TxnType")
             .HasFilter("[IsDeleted] = 0");

            // BaseEntity fields mapping (adjust names/types to your BaseEntity)
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
        }
    }
}