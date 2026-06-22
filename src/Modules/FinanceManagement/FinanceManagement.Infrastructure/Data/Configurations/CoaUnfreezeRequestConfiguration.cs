using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class CoaUnfreezeRequestConfiguration : IEntityTypeConfiguration<CoaUnfreezeRequest>
    {
        public void Configure(EntityTypeBuilder<CoaUnfreezeRequest> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("CoaUnfreezeRequest", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.Reason).HasColumnName("Reason").HasColumnType("varchar(1000)").IsRequired();
            builder.Property(t => t.CfoApproverUserId).HasColumnName("CfoApproverUserId").HasColumnType("int");
            builder.Property(t => t.CfoApprovedOn).HasColumnName("CfoApprovedOn");
            builder.Property(t => t.SysAdminApproverUserId).HasColumnName("SysAdminApproverUserId").HasColumnType("int");
            builder.Property(t => t.SysAdminApprovedOn).HasColumnName("SysAdminApprovedOn");
            builder.Property(t => t.RequestStatus).HasColumnName("Status").HasColumnType("varchar(30)").IsRequired();
            builder.Property(t => t.WindowMinutes).HasColumnName("WindowMinutes").HasColumnType("int").IsRequired();
            builder.Property(t => t.WindowOpenedOn).HasColumnName("WindowOpenedOn");
            builder.Property(t => t.WindowExpiry).HasColumnName("WindowExpiry");
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

            // The lapsing job scans open windows by company + status + expiry.
            builder.HasIndex(t => new { t.CompanyId, t.RequestStatus });
        }
    }
}
