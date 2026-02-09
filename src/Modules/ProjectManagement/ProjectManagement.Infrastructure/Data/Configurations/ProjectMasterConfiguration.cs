using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static ProjectManagement.Domain.Common.BaseEntity;



namespace ProjectManagement.Infrastructure.Data.Configurations
{
    public class ProjectMasterConfiguration  : IEntityTypeConfiguration<ProjectMaster>
    {
              public void Configure(EntityTypeBuilder<ProjectMaster> builder)
              {

                     var statusConverter = new ValueConverter<Status, bool>(
                        v => v == Status.Active,                    // Convert to DB (1 for Active)
                        v => v ? Status.Active : Status.Inactive    // Convert to Entity
                    );

                     // ValueConverter for IsDelete (enum to bit)
                     var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                         v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                         v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
                     );

                     builder.ToTable("ProjectMaster", "Project");

                     builder.HasKey(m => m.Id);
                     builder.Property(m => m.Id)
                      .HasColumnName("Id")
                      .HasColumnType("int")
                      .ValueGeneratedOnAdd()
                      .IsRequired();

                     builder.Property(p => p.ProjectCode)
                            .IsRequired()
                            .HasMaxLength(50);

                     builder.HasIndex(p => p.ProjectCode)
                            .IsUnique();

                     builder.Property(p => p.ProjectName)
                            .IsRequired()
                            .HasMaxLength(200);

                     builder.Property(p => p.ProjectDescription)
                            .HasMaxLength(2000);

                     builder.Property(p => p.ProjectTypeId)
                            .IsRequired();

                     builder.Property(p => p.UnitId)
                            .IsRequired();

                     builder.Property(p => p.DepartmentId)
                            .IsRequired();

                     // --- Financial Information ---
                     builder.Property(p => p.BudgetAmount)
                            .HasColumnType("decimal(18,2)")
                            .IsRequired();

                     builder.Property(p => p.BudgetYearId)
                            .IsRequired();

                     builder.Property(p => p.CostCenterId)
                            .IsRequired();

                     builder.Property(p => p.CurrencyId)
                            .IsRequired();

                     // --- Timeline ---
                     builder.Property(p => p.StartDate)
                            .IsRequired();

                     builder.Property(p => p.EndDate)
                            .IsRequired();

                     // --- Capex / Asset Related ---
                     builder.Property(p => p.ProjectCategoryId)
                            .IsRequired();

                     builder.Property(p => p.AssetGroupId)
                            .IsRequired();

                     builder.Property(p => p.PurposeRemarks)
                            .IsRequired(false)
                            .HasMaxLength(1000);

                     builder.Property(p => p.StatusId)
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
                
                            builder.HasMany(p => p.ProjectDocuments)
                     .WithOne(d => d.Project)
                     .HasForeignKey(d => d.ProjectId)
                     .OnDelete(DeleteBehavior.Cascade);

   
        }
    }
}