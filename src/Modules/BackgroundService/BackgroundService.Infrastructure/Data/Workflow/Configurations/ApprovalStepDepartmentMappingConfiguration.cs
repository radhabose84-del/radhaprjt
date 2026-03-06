using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalStepDepartmentMappingConfiguration : IEntityTypeConfiguration<ApprovalStepDepartmentMapping>
    {
        public void Configure(EntityTypeBuilder<ApprovalStepDepartmentMapping> builder)
        {
            builder.ToTable("ApprovalStepDepartmentMapping", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired(true);

            builder.Property(t => t.ApprovalStepDetailId)
            .HasColumnName("ApprovalStepDetailId")
            .HasColumnType("int")
            .IsRequired(true);

            builder.Property(t => t.DepartmentId)
            .HasColumnName("DepartmentId")
            .HasColumnType("int")
            .IsRequired(true);
            
              builder.HasOne(ac => ac.ApprovalStepDetail)
          .WithMany(am => am.ApprovalStepDepartmentMappings)
          .HasForeignKey(ac => ac.ApprovalStepDetailId)
          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}