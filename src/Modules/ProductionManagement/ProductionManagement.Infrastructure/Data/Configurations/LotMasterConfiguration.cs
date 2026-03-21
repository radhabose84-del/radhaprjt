using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class LotMasterConfiguration : IEntityTypeConfiguration<LotMaster>
    {
        public void Configure(EntityTypeBuilder<LotMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("LotMaster", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.LotCode).HasColumnName("LotCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.BatchNumber).HasColumnName("BatchNumber").HasColumnType("varchar(50)").IsRequired();
            builder.Property(t => t.LotTypeId).HasColumnName("LotTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnName("ItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ProductionOrderRef).HasColumnName("ProductionOrderRef").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.TotalProducedQty).HasColumnName("TotalProducedQty").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.AvailableQty).HasColumnName("AvailableQty").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.RunOutDate).HasColumnName("RunOutDate").HasColumnType("date").IsRequired(false);
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasIndex(t => t.LotCode).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.LotTypeId);

            builder.HasOne(t => t.LotTypeMisc).WithMany(m => m.LotMastersAsLotType).HasForeignKey(t => t.LotTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.StatusMisc).WithMany(m => m.LotMastersAsStatus).HasForeignKey(t => t.StatusId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
