using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalStepUnitMappingConfiguration : IEntityTypeConfiguration<ApprovalStepUnitMapping>
    {
        public void Configure(EntityTypeBuilder<ApprovalStepUnitMapping> builder)
        {
            builder.ToTable("ApprovalStepUnitMapping", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired(true);

            builder.Property(t => t.ApprovalStepDetailId)
            .HasColumnName("ApprovalStepDetailId")
            .HasColumnType("int")
            .IsRequired(true);

            builder.Property(t => t.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired(true);
            
              builder.HasOne(ac => ac.ApprovalStepDetail)
          .WithMany(am => am.ApprovalStepUnitMappings)
          .HasForeignKey(ac => ac.ApprovalStepDetailId)
          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}