using FAM.Domain.Entities.AssetMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FAM.Infrastructure.Data.Configurations.AssetMaster
{
    public class AssetAuditConfiguration : IEntityTypeConfiguration<AssetAudit>
    {
        public void Configure(EntityTypeBuilder<AssetAudit> builder)
        {
            builder.ToTable("AssetAudit", "FixedAsset");
            // Primary Key
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.AssetCode)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.AssetName)
               .HasColumnType("varchar(100)");

            builder.Property(b => b.UnitName)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.Department)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.AuditorName)
                .HasColumnType("varchar(100)");

            builder.Property(dg => dg.AuditDate)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.AuditFinancialYear)
                .HasColumnType("varchar(100)");

            builder.Property(dg => dg.AuditTypeId)
                .HasColumnType("int")
                .IsRequired();

            // Configure Foreign Key Relationship
            builder.HasOne(dg => dg.AuditTypeMiscType)
                .WithMany(ag => ag.AssetAuditType)
                .HasForeignKey(dg => dg.AuditTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.UploadedFileId)
                .HasColumnType("int");

            builder.Property(b => b.SourceFileName)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.ScanType)
                .HasColumnType("varchar(10)");

            builder.Property(b => b.Status)
                .HasColumnType("varchar(10)");

            builder.Property(b => b.UnitId)
                .HasColumnType("int")
                 .IsRequired();

            builder.Property(b => b.CreatedBy)
                .HasColumnType("int");

            builder.Property(dg => dg.CreatedDate)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

  
        }
    }
}