

using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations
{
    public class MachineGroupUserConfiguration  : IEntityTypeConfiguration<MachineGroupUser>
    {
        public void Configure(EntityTypeBuilder<MachineGroupUser> builder)
        {
                var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                   
                v => v ? Status.Active : Status.Inactive   
            );
                
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
            );

            builder.ToTable("MachineGroupUser", "Maintenance");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.MachineGroupId)
            .IsRequired()
            .HasColumnType("int");

            builder.HasOne(s => s.MachineGroup)
            .WithMany(c => c.MachineGroupUser)
            .HasForeignKey(s => s.MachineGroupId)
            .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(s => s.DepartmentId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(s => s.UserId)
            .IsRequired()
            .HasColumnType("int"); 

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
            .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
            .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
            .HasColumnType("varchar(255)");              
        }
    }
}