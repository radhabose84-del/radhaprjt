using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Common.BaseEntity;

namespace ProjectManagement.Infrastructure.Data.Configurations
{
    public class ProjectWorkBreakdownStructureConfiguration : IEntityTypeConfiguration<ProjectWorkBreakdownStructure>
    {
        public void Configure(EntityTypeBuilder<ProjectWorkBreakdownStructure> builder)
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

             builder.ToTable("ProjectWorkBreakdownStructure", "Project");

            // Primary Key
            builder.HasKey(x => x.Id);       

            builder.Property(x => x.ProjectId)
                .IsRequired();
            builder.Property(x => x.ParentWorkBreakdownStructureId)
                .IsRequired(false);
            
            builder.Property(x => x.WorkBreakdownStructureName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.WorkBreakdownStructureDescription)
                .HasMaxLength(1000)
                .IsRequired(false);           

            builder.Property(x => x.StartDate)
                .IsRequired(false);   // nullable in entity

            builder.Property(x => x.EndDate)
                .IsRequired(false);

            builder.Property(x => x.DurationInDays)
                .IsRequired(false);   // derived, can be null if dates missing          

            builder.Property(x => x.ResponsibleDepartmentId)
                .IsRequired();
            builder.Property(x => x.ResponsiblePerson)
                .HasMaxLength(200)
                .IsRequired();
           

            builder.Property(x => x.CostCenterId)
                .IsRequired(false);

            builder.Property(x => x.PlannedBudgetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(x => x.CurrencyId)        
                    .IsRequired();           

            builder.Property(x => x.IsMilestone)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.MilestoneDate)
                .IsRequired(false);          

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000)
                .IsRequired(false);           

            builder.Property(x => x.StatusId)
                .IsRequired();

            builder.Property(x => x.Level)
                .IsRequired();

            builder.Property(x => x.UnitId)
                .IsRequired();

            builder.Property(x => x.BudgetYearId)
                .IsRequired();            

            // Unique WBS name within same project
            builder.HasIndex(x => new { x.ProjectId, x.WorkBreakdownStructureName })
                .IsUnique();

            // (Optional) often useful to index ProjectId for queries
            builder.HasIndex(x => x.ProjectId);

            // ---------------- Relationships ----------------

            // Project relationship (1 Project -> many WBS)
            builder.HasOne(x => x.Project)
                .WithMany(p => p.ProjectWorkBreakdownStructures)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Self-referencing parent-child hierarchy
            builder.HasOne(x => x.ParentWorkBreakdownStructure)
                .WithMany(p => p.ChildWorkBreakdownStructures)
                .HasForeignKey(x => x.ParentWorkBreakdownStructureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}