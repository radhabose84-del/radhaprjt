using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class EntityConfigurations : IEntityTypeConfiguration<Core.Domain.Entities.Entity>
    {
        public void Configure(EntityTypeBuilder<Entity> builder)
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

        builder.ToTable("Entity", "AppData");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(u => u.EntityCode)
            .HasColumnName("EntityCode")
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(u => u.EntityName)
            .HasColumnName("EntityName")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(u => u.EntityDescription)
            .HasColumnName("EntityDescription")
            .HasColumnType("varchar(250)")
            .IsRequired();

        builder.Property(u => u.Address)
            .HasColumnName("Address")
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(40)")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(200)")
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
        }
    }
}