using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
        {
            builder.ToTable("ApprovalRequest", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.WorkflowType)
            .HasColumnName("WorkflowType")
            .HasColumnType("nvarchar(200)")
            .IsRequired();

            builder.Property(t => t.WorkflowTypeId)
            .HasColumnName("WorkflowTypeId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.ModuleTransactionId)
            .HasColumnName("ModuleTransactionId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.ApprovalStepDetailId)
            .HasColumnName("ApprovalStepDetailId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.ApprovalRuleId)
            .HasColumnName("ApprovalRuleId")
            .HasColumnType("int")
            .IsRequired(false);

            builder.Property(t => t.StatusId)
           .HasColumnName("StatusId")
           .HasColumnType("int")
           .IsRequired();

            builder.Property(t => t.RequestedDate)
            .HasColumnName("RequestedDate")
            .HasColumnType("datetimeoffset")
            .IsRequired();

            builder.Property(t => t.UnitId)
           .HasColumnName("UnitId")
           .HasColumnType("int")
           .IsRequired();

            builder.Property(t => t.DepartmentId)
            .HasColumnName("DepartmentId")
            .HasColumnType("int")
            .IsRequired(false);

            builder.Property(t => t.Action)
            .HasColumnName("Action")
            .HasColumnType("Varchar(50)")
            .IsRequired(false);

            builder.Property(t => t.Remark)
            .HasColumnName("Remark")
            .HasColumnType("Varchar(max)")
            .IsRequired(false);


            builder.Property(t => t.ApproverBinding)
         .HasColumnName("ApproverBinding")
         .HasColumnType("nvarchar(10)")
         .IsRequired();

            builder.Property(t => t.ApproverValue)
        .HasColumnName("ApproverValue")
        .HasColumnType("nvarchar(200)")
        .IsRequired();

            builder.Property(cf => cf.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(cf => cf.ModifiedIP)
                .HasColumnType("varchar(255)");


            builder.HasOne(ac => ac.ApprovalStepDetail)
           .WithMany(am => am.ApprovalRequest)
           .HasForeignKey(ac => ac.ApprovalStepDetailId)
           .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ac => ac.ApprovalRule)
            .WithMany(am => am.ApprovalRequest)
            .HasForeignKey(ac => ac.ApprovalRuleId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ac => ac.Status)
           .WithMany(am => am.ApprovalRequestStatus)
           .HasForeignKey(ac => ac.StatusId)
           .OnDelete(DeleteBehavior.NoAction);
          
          builder.HasOne(ac => ac.WType)
          .WithMany(am => am.ApprovalRequests)
          .HasForeignKey(ac => ac.WorkflowTypeId)
          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}