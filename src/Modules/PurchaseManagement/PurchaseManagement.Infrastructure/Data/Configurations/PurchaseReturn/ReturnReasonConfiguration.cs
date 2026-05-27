using PurchaseManagement.Domain.Entities.PurchaseReturn;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseReturn;

public class ReturnReasonConfiguration : IEntityTypeConfiguration<ReturnReason>
{
    public void Configure(EntityTypeBuilder<ReturnReason> b)
    {
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive);

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

        b.ToTable("ReturnReason", schema: "Purchase");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasColumnType("varchar(30)").IsRequired();
        b.Property(x => x.Description).HasColumnType("varchar(150)").IsRequired();
        b.Property(x => x.ReturnTypeId).IsRequired();
        b.Property(x => x.IsReplacementOverride).HasColumnType("bit").IsRequired(false);
        b.Property(x => x.IsDebitNoteOverride).HasColumnType("bit").IsRequired(false);
        b.Property(x => x.IsQcMandatoryOverride).HasColumnType("bit").IsRequired(false);

        b.HasOne(x => x.ReturnType)
         .WithMany(t => t.Reasons)
         .HasForeignKey(x => x.ReturnTypeId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_ReturnReason_Code");
        b.HasIndex(x => x.ReturnTypeId).HasDatabaseName("IX_ReturnReason_ReturnTypeId");

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
