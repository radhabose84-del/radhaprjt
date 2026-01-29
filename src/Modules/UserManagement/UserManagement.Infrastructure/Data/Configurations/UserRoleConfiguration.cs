using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserRoleConfiguration :IEntityTypeConfiguration<UserRole>
    {
         public void Configure(EntityTypeBuilder<UserRole> builder)
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

               builder.ToTable("UserRole", "AppSecurity");

            builder.HasKey(u => u.Id);

              builder.Property(u => u.Id)
                        .HasColumnName("Id")
                        .HasColumnType("int")
                        .IsRequired();

             builder.Property(u => u.RoleName)
            .HasColumnName("RoleName")
            .HasColumnType("varchar(50)")
            .IsRequired();

             builder.Property(u => u.Description)
            .HasColumnName("Description")
            .HasColumnType("varchar(250)")
            .IsRequired();

              builder.Property(u => u.CompanyId)
            .HasColumnName("CompanyId")
            .HasColumnType("int")
            .IsRequired();

             builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

                 builder.Property(u => u.IsDeleted)
            .HasColumnName("IsDeleted")
            .HasColumnType("bit")
            .HasConversion(isDeletedConverter)
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

            builder.HasMany(ur => ur.UserRoleAllocations)
                .WithOne(ura => ura.UserRole)
                .HasForeignKey(ura => ura.UserRoleId);

                builder.HasMany(ur => ur.RoleModules)
                .WithOne(rm => rm.Role)
                .HasForeignKey(ura => ura.RoleId);

               
                
            // builder.HasMany(ur => ur.RoleEntitlements)
            // .WithOne(re => re.UserRole)
            // .HasForeignKey(re => re.UserRoleId)
            // .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}