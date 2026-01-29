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
    public class ApprovalRuleConfiguration : IEntityTypeConfiguration<ApprovalRule>
    {
        public void Configure(EntityTypeBuilder<ApprovalRule> builder)
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

            builder.ToTable("ApprovalRule", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ApprovalStepDetailId)
            .HasColumnName("ApprovalStepDetailId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.Priority)
            .HasColumnName("Priority")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.ActionId)
            .HasColumnName("ActionId")
            .HasColumnType("int")
            .IsRequired();


            builder.Property(t => t.EffectiveFrom)
            .HasColumnName("EffectiveFrom")
            .HasColumnType("date")
            .IsRequired(true);

            builder.Property(t => t.EffectiveTo)
           .HasColumnName("EffectiveTo")
           .HasColumnType("date")
           .IsRequired(true);

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


            builder.HasOne(ac => ac.ApprovalStepDetail)
           .WithMany(am => am.ApprovalRules)
           .HasForeignKey(ac => ac.ApprovalStepDetailId)
           .OnDelete(DeleteBehavior.NoAction);
          
             builder.HasOne(ac => ac.Action)
          .WithMany(am => am.Action)
          .HasForeignKey(ac => ac.ActionId)
          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}