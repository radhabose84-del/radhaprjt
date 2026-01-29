using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class DepartmentConfiguration :IEntityTypeConfiguration<Department>
    {
         public void Configure(EntityTypeBuilder<Department> builder)
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

              builder.ToTable("Department", "AppData");

            builder.HasKey(u => u.Id);

              builder.Property(u => u.Id)
                        .HasColumnName("Id")
                        .HasColumnType("int")
                        .IsRequired();

             
             builder.Property(u => u.ShortName)
            .HasColumnName("ShortName")
            .HasColumnType("varchar(10)")
            .IsRequired();

             builder.Property(u => u.DeptName)
            .HasColumnName("DeptName")
            .HasColumnType("varchar(50)")
            .IsRequired();


            builder.Property(u => u.DepartmentGroupId)
                      .HasColumnName("DepartmentGroupId")
                      .HasColumnType("int")
                      .IsRequired();

            builder.HasOne(d => d.DepartmentGroup)
             .WithMany(g => g.Departments)
             .HasForeignKey(d => d.DepartmentGroupId)            
            .OnDelete(DeleteBehavior.Restrict);

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

            

           



        }


    }
}