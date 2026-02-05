using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyGroupConfiguration : IEntityTypeConfiguration<PartyGroup>
    {
        public void Configure(EntityTypeBuilder<PartyGroup> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            builder.ToTable("PartyGroup", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(pg => pg.PartyGroupName)
                    .HasColumnName("PartyGroupName")
                    .IsRequired()
                   .HasColumnType("varchar(100)");

            builder.Property(m => m.ParentPartyGroupId)  // Foreign Key column
              .HasColumnName("ParentPartyGroupId")
              .HasColumnType("int");  // Set as int

              // Foreign Key Relationship
            builder.HasOne(m => m.ParentPartyGroup)
                .WithMany(t => t.ChildPartyGroups)
                .HasForeignKey(m => m.ParentPartyGroupId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed  

            builder.Property(m => m.GroupTypeId)  // Foreign Key column
              .HasColumnName("GroupTypeId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GroupType)
                .WithMany(t => t.PartyGroupTypes)
                .HasForeignKey(m => m.GroupTypeId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            // Description column
            builder.Property(m => m.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)");

            
            // Description column
            builder.Property(m => m.Glcode)
                .HasColumnName("Glcode")
                .HasColumnType("varchar(10)");


              builder.Property(m => m.GlCategoryId)  // Foreign Key column
              .HasColumnName("GlCategoryId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GlCategory)
                .WithMany(t => t.PartyGlCategoryCode)
                .HasForeignKey(m => m.GlCategoryId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed
                

            builder.Property(t => t.IsGroup)
                .HasColumnName("IsGroup")
                .HasColumnType("bit")
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
                .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(255)");
            

        }
    }
}