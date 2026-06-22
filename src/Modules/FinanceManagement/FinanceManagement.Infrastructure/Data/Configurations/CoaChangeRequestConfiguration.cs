using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class CoaChangeRequestConfiguration : IEntityTypeConfiguration<CoaChangeRequest>
    {
        public void Configure(EntityTypeBuilder<CoaChangeRequest> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CoaChangeRequest", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.TargetAccountId).HasColumnName("TargetAccountId").HasColumnType("int");
            builder.Property(t => t.TargetAccountGroupId).HasColumnName("TargetAccountGroupId").HasColumnType("int");
            builder.Property(t => t.AccountCodeSnapshot).HasColumnName("AccountCodeSnapshot").HasColumnType("varchar(50)");
            builder.Property(t => t.ChangeType).HasColumnName("ChangeType").HasColumnType("varchar(50)").IsRequired();
            builder.Property(t => t.Justification).HasColumnName("Justification").HasColumnType("varchar(1000)").IsRequired();
            builder.Property(t => t.ImpactAssessment).HasColumnName("ImpactAssessment").HasColumnType("varchar(2000)").IsRequired();
            builder.Property(t => t.ImpactApprovedByUserId).HasColumnName("ImpactApprovedByUserId").HasColumnType("int");
            builder.Property(t => t.ImpactApprovedOn).HasColumnName("ImpactApprovedOn");
            builder.Property(t => t.UnfreezeRequestId).HasColumnName("UnfreezeRequestId").HasColumnType("int");
            builder.Property(t => t.RequestStatus).HasColumnName("Status").HasColumnType("varchar(30)").IsRequired();
            builder.Property(t => t.IsPostFreeze).HasColumnName("IsPostFreeze").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.CommittedByUserId).HasColumnName("CommittedByUserId").HasColumnType("int");
            builder.Property(t => t.CommittedOn).HasColumnName("CommittedOn");
            builder.Property(t => t.RequestedByUserId).HasColumnName("RequestedByUserId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RequestedOn).HasColumnName("RequestedOn");

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // The lapsing job and the change-log read-model query by window + status.
            builder.HasIndex(t => new { t.UnfreezeRequestId, t.RequestStatus });
            builder.HasIndex(t => new { t.CompanyId, t.RequestStatus });

            // The window this request is batched under (same-module FK, no cascade — lapsing is explicit).
            builder.HasOne(t => t.UnfreezeRequest)
                .WithMany(u => u.ChangeRequests)
                .HasForeignKey(t => t.UnfreezeRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
