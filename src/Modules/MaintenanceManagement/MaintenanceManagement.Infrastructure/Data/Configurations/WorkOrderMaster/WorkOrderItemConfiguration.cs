using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster
{
    public class WorkOrderItemConfiguration  : IEntityTypeConfiguration<WorkOrderItem>
    {
        public void Configure(EntityTypeBuilder<WorkOrderItem> builder)
        {
            builder.ToTable("WorkOrderItem", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkOrderId)
                .HasColumnName("WorkOrderId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.WOItem)
                .WithMany(am => am.WorkOrderItems)
                .HasForeignKey(amg => amg.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.StoreTypeId)
                .HasColumnName("StoreTypeId")
                .HasColumnType("int")
                .IsRequired(false);
            builder.HasOne(amg => amg.MiscStoreType)
                .WithMany(am => am.WorkOrderItemStoreType)
                .HasForeignKey(amg => amg.StoreTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.ItemCode)
                .HasColumnName("ItemCode")
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(t => t.OldItemCode)
                .HasColumnName("OldItemCode")
                .HasColumnType("nvarchar(100)")
                .IsRequired(false);

            builder.Property(t => t.ItemName)
                .HasColumnName("ItemName")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            builder.Property(t => t.AvailableQty)
                .HasColumnName("AvailableQty")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UsedQty)
                .HasColumnName("UsedQty")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ScarpQty)
                .HasColumnName("ScarpQty")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ToSubStoreQty)
                .HasColumnName("ToSubStoreQty")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Image)
                .HasColumnName("Image")
                .HasColumnType("varchar(250)")
                .IsRequired(false);  
                
            builder.Property(t => t.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired(false);                  
        }
    }
}