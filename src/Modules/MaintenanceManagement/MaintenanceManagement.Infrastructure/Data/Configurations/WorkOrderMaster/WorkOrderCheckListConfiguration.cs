
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster
{
    public class WorkOrderCheckListConfiguration  : IEntityTypeConfiguration<WorkOrderCheckList>
    {       
        public void Configure(EntityTypeBuilder<WorkOrderCheckList> builder)
        {
            builder.ToTable("WorkOrderCheckList", "Maintenance");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkOrderId)
                .HasColumnName("WorkOrderId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.WOCheckList)
                .WithMany(am => am.WorkOrderCheckLists)
                .HasForeignKey(amg => amg.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(t => t.CheckListId)
                .HasColumnName("CheckListId")
                .HasColumnType("int")
                .IsRequired();
            builder.HasOne(amg => amg.CheckListMaster)
                .WithMany(am => am.WOCheckLists)
                .HasForeignKey(amg => amg.CheckListId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.ISCompleted)                
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")                
                .HasColumnType("varchar(1000)")
                .IsRequired(false);      
        }
    }
}