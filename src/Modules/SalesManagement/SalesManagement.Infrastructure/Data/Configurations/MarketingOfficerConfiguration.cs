using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class MarketingOfficerConfiguration : IEntityTypeConfiguration<MarketingOfficer>
    {
        public void Configure(EntityTypeBuilder<MarketingOfficer> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("MarketingOfficer", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EmployeeNo)
                .HasColumnName("EmployeeNo")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.EmployeeName)
                .HasColumnName("EmployeeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.MobileNo)
                .HasColumnName("MobileNo")
                .HasColumnType("varchar(15)")
                .IsRequired(false);

            builder.Property(t => t.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(t => t.Unit)
                .HasColumnName("Unit")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Department)
                .HasColumnName("Department")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Designation)
                .HasColumnName("Designation")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.SalesOfficeId)
                .HasColumnName("SalesOfficeId")
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.EmployeeNo).IsUnique();
            builder.HasIndex(t => t.SalesOfficeId);

            // FK: MarketingOfficer → SalesOffice (same module, Sales schema)
            builder.HasOne(t => t.SalesOffice)
                .WithMany(o => o.MarketingOfficers)
                .HasForeignKey(t => t.SalesOfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
