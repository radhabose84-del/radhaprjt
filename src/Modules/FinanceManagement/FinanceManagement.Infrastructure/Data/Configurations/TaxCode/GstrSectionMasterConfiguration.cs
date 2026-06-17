using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GstrSectionMasterConfiguration : IEntityTypeConfiguration<GstrSectionMaster>
    {
        public void Configure(EntityTypeBuilder<GstrSectionMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            builder.ToTable("GstrSectionMaster", "Finance");
            builder.HasKey(t => t.Id);

            // No soft delete — "remove" = IsActive = Inactive (consistent with the tax-code entities).
            builder.Ignore(b => b.IsDeleted);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ReportTypeId).HasColumnName("ReportTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SectionCode).HasColumnName("SectionCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.SectionName).HasColumnName("SectionName").HasColumnType("varchar(200)").IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // One section code per (company, report).
            builder.HasIndex(t => new { t.CompanyId, t.ReportTypeId, t.SectionCode })
                .IsUnique()
                .HasFilter("[IsActive] = 1")
                .HasDatabaseName("UX_GstrSectionMaster_Company_Report_Code");
            builder.HasIndex(t => t.ReportTypeId).HasDatabaseName("IX_GstrSectionMaster_ReportTypeId");

            builder.HasOne(t => t.ReportType)
                .WithMany()
                .HasForeignKey(t => t.ReportTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
