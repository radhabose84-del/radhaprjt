using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<Cities>
    {
        public void Configure(EntityTypeBuilder<Cities> builder)
        {
             var isActiveConverter = new ValueConverter<Status, bool>
               (
                    v => v == Status.Active,  
                    v => v ? Status.Active : Status.Inactive 
                );

                var isDeletedConverter = new ValueConverter<IsDelete, bool>
                (
                 v => v == IsDelete.Deleted,  
                 v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
                );

                builder.ToTable("City", "AppData");
                builder.HasKey(u => u.Id);
                //builder.HasKey(u => new { u.Id, u.StateCode });

                builder.Property(u => u.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(u => u.CityCode)
                .HasColumnName("CityCode")
                .HasColumnType("varchar(5)")
                .IsRequired();

                builder.Property(u => u.CityName)
                .HasColumnName("CityName")
                .HasColumnType("varchar(50)")
                .IsRequired();
                
                builder.Property(u => u.StateId)
                .HasColumnName("StateId")
                .HasColumnType("int")
                .IsRequired();

                builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

                builder.Property(u => u.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasColumnType("int");   

                builder.Property(u => u.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("datetime");

                builder.Property(u => u.CreatedByName)
                .HasColumnName("CreatedByName")
                .HasColumnType("varchar(50)");

                builder.Property(u => u.CreatedIP)
                .HasColumnName("CreatedIP")
                .HasColumnType("varchar(25)");

                builder.Property(u => u.ModifiedAt)
                .HasColumnName("ModifiedAt")
                .HasColumnType("datetime");

                builder.Property(u => u.ModifiedBy)
                .HasColumnName("ModifiedBy")
                .HasColumnType("int");

                builder.Property(u => u.ModifiedByName)
                .HasColumnName("ModifiedByName")
                .HasColumnType("varchar(50)");


                builder.Property(u => u.ModifiedIP)
                .HasColumnName("ModifiedIP")
                .HasColumnType("varchar(25)"); 
                
                builder.Property(u => u.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeletedConverter)
                .IsRequired();
                // Configure the foreign key relationship
                builder.HasOne(s => s.States)
                .WithMany(c => c.Cities)
                .HasForeignKey(s => s.StateId)
                .OnDelete(DeleteBehavior.Restrict);            
        }
    }
}