using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data.Notification.Configurations
{
    public class MiscMasterConfiguration : IEntityTypeConfiguration<MiscMaster>
    {
        public void Configure(EntityTypeBuilder<MiscMaster> builder)
        {
              
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                   
                v => v ? Status.Active : Status.Inactive    
            );

                
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
            );
            
            builder.ToTable("MiscMaster", "AppData");
          
            builder.Property(m => m.MiscTypeId)  
                .HasColumnName("MiscTypeId")
                .HasColumnType("int")  
                .IsRequired();

        builder.Property(m => m.Code)
               .HasColumnName("Code")
               .HasColumnType("nvarchar(50)")  
               .IsRequired();

               
        builder.Property(m => m.Description)
               .HasColumnName("description")
               .HasColumnType("varchar(250)")
               .IsRequired();

        builder.Property(m => m.SortOrder)
               .HasColumnName("sortOrder")  
               .HasColumnType("int")
               .HasDefaultValue(0)
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

       // Foreign Key Relationship
            builder.HasOne(m => m.MiscType)
                .WithMany(t => t.MiscMaster)
                .HasForeignKey(m => m.MiscTypeId) 
                .HasPrincipalKey(t => t.Id)  
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}