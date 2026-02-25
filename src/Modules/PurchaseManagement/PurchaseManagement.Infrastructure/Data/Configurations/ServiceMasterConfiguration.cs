using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class ServiceMasterConfiguration : IEntityTypeConfiguration<ServiceMaster>
    {

        public void Configure(EntityTypeBuilder<ServiceMaster> builder)
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

            builder.ToTable("ServiceMaster", "Purchase");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ServiceCode)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate()
            .HasComputedColumnSql(
                "('SRV' + REPLICATE('0', CASE WHEN LEN(CONVERT(varchar(10), [Id])) < 4 " +
                "THEN 4 - LEN(CONVERT(varchar(10), [Id])) ELSE 0 END) + CONVERT(varchar(10), [Id]))",
                stored: true);

            // builder.Property(x => x.ServiceCode)
            // .HasColumnType("nvarchar(50)")                       
            // .IsRequired();
            // builder.Property(x => x.ServiceCode)
            // .HasColumnType("nvarchar(50)")
            // .HasMaxLength(50)
            // .IsRequired()
            // .ValueGeneratedOnAddOrUpdate()
            // .HasComputedColumnSql(
            //     "('SRV' + RIGHT(REPLICATE('0',4) + CONVERT(varchar(10), [Id]), 4))",
            //  stored: true);

            builder.Property(x => x.ServiceDescription)
            .HasColumnType("nvarchar(200)")
            .IsRequired();

            builder.Property(x => x.SacId)
            .HasColumnType("int")
            .IsRequired();

            builder.Property(x => x.UomId)
            .HasColumnType("int")
            .IsRequired();

            builder.Property(x => x.ServiceCategoryId)
            .HasColumnType("int")
            .IsRequired();

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