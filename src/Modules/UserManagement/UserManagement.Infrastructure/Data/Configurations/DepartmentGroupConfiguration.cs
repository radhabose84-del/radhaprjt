using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class DepartmentGroupConfiguration :IEntityTypeConfiguration<DepartmentGroup>
    {
        public void Configure(EntityTypeBuilder<DepartmentGroup> builder)
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

            builder.ToTable("DepartmentGroup", "AppData");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                      .HasColumnName("Id")
                      .HasColumnType("int")
                      .IsRequired();

            builder.Property(u => u.DepartmentGroupCode)
                      .HasColumnName("DepartmentGroupCode")
                      .HasColumnType("varchar(15)")
                      .IsRequired();

            builder.Property(u => u.DepartmentGroupName)
                     .HasColumnName("DepartmentGroupName")
                     .HasColumnType("varchar(50)")
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

            builder.Property(u => u.CreatedBy)
                     .HasColumnName("CreatedBy")
                     .HasColumnType("int")
                     .IsRequired();

            builder.Property(u => u.CreatedAt)
                     .HasColumnName("CreatedDate")
                     .HasColumnType("datetime")
                     .IsRequired();


            builder.Property(u => u.ModifiedBy)
                     .HasColumnName("ModifiedBy")
                     .HasColumnType("int")
                     .IsRequired(false);

            builder.Property(u => u.ModifiedAt)
                     .HasColumnName("ModifiedDate")
                     .HasColumnType("datetime")
                     .IsRequired(false);







        }
    }
}