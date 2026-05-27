using PurchaseManagement.Domain.Entities.PurchaseReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseReturn;

public class ReturnTypeConfiguration : IEntityTypeConfiguration<ReturnType>
{
    public void Configure(EntityTypeBuilder<ReturnType> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive);

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

        b.ToTable("ReturnType", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasColumnType("varchar(30)").IsRequired();
        b.Property(x => x.Description).HasColumnType("varchar(100)").IsRequired();
        b.Property(x => x.InventoryImpactId).IsRequired(false);
        b.Property(x => x.FinanceImpactId).IsRequired(false);
        b.Property(x => x.IsReplacementApplicable).HasColumnType("bit").IsRequired();
        b.Property(x => x.IsQcMandatory).HasColumnType("bit").IsRequired();
        b.Property(x => x.ApprovalRoleCode).HasColumnType("varchar(30)").IsRequired(false);

        b.HasOne(x => x.InventoryImpact)
         .WithMany()
         .HasForeignKey(x => x.InventoryImpactId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.FinanceImpact)
         .WithMany()
         .HasForeignKey(x => x.FinanceImpactId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReturnType_Code");
        b.HasIndex(x => x.InventoryImpactId).HasDatabaseName("IX_ReturnType_InventoryImpactId");
        b.HasIndex(x => x.FinanceImpactId).HasDatabaseName("IX_ReturnType_FinanceImpactId");

        // BaseEntity audit columns
        b.Property(x => x.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
        b.Property(x => x.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int").IsRequired();
        b.Property(x => x.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetimeoffset").IsRequired();
        b.Property(x => x.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)").IsRequired();
        b.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int").IsRequired(false);
        b.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate").HasColumnType("datetimeoffset").IsRequired(false);
        b.Property(x => x.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(50)").IsRequired(false);
        b.Property(x => x.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)").IsRequired(false);
    }
}
