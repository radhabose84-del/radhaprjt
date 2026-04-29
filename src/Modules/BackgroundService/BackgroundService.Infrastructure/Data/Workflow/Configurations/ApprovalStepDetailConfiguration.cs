using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalStepDetailConfiguration : IEntityTypeConfiguration<ApprovalStepDetail>
    {
        public void Configure(EntityTypeBuilder<ApprovalStepDetail> builder)
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

            builder.ToTable("ApprovalStepDetail", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkFlowTypeId)
            .HasColumnName("WorkFlowTypeId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.StepOrder)
            .HasColumnName("StepOrder")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.StopOnFirstMatch)
           .HasColumnName("StopOnFirstMatch")
           .HasColumnType("bit")
              .HasConversion(
        v => v == 1,
        v => v ? (byte)1 : (byte)0
         )
            .IsRequired();

            builder.Property(t => t.ApprovalStepId)
            .HasColumnName("ApprovalStepId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.TargetTypeId)
           .HasColumnName("TargetTypeId")
           .HasColumnType("int")
           .IsRequired();

            builder.Property(t => t.TargetValueId)
            .HasColumnName("TargetValueId")
            .HasColumnType("int")
            .IsRequired(false);

            builder.Property(t => t.IsEdit)
            .HasColumnName("IsEdit")
             .HasColumnType("bit")
              .HasConversion(
            v => v == 1,
            v => v ? (byte)1 : (byte)0
            )
                .IsRequired();;


            builder.Property(cf => cf.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasConversion(isActiveConverter)
            .IsRequired();

            builder.Property(cf => cf.IsDeleted)
                 .HasColumnName("IsDeleted")
                 .HasColumnType("bit")
                 .HasConversion(isDeletedConverter)
                 .IsRequired();

            builder.Property(cf => cf.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(cf => cf.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(cf => cf.ModifiedByName)
                 .HasColumnType("varchar(50)");

            builder.Property(cf => cf.ModifiedIP)
                .HasColumnType("varchar(255)");

            builder.HasOne(ac => ac.WorkflowType)
     .WithMany(am => am.ApprovalStepDetails)
     .HasForeignKey(ac => ac.WorkFlowTypeId)
     .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ac => ac.ApprovalStep)
            .WithMany(am => am.ApprovalStep)
            .HasForeignKey(ac => ac.ApprovalStepId)
            .OnDelete(DeleteBehavior.NoAction);
            
            builder.HasOne(ac => ac.TargetType)
            .WithMany(am => am.ApprovalTargetType)
            .HasForeignKey(ac => ac.TargetTypeId)
            .OnDelete(DeleteBehavior.NoAction);
          
        }
    }
}