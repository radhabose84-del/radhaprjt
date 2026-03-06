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
    public class RuleTargetOverrideConfiguration : IEntityTypeConfiguration<RuleTargetOverride>
    {
        public void Configure(EntityTypeBuilder<RuleTargetOverride> builder)
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

            builder.ToTable("RuleTargetOverride", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RuleId)
            .HasColumnName("RuleId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.Binding)
            .HasColumnName("Binding")
            .HasColumnType("nvarchar(10)")
            .IsRequired();

            builder.Property(t => t.Value)
            .HasColumnName("Value")
            .HasColumnType("nvarchar(200)")
            .IsRequired();


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

          
           builder.HasOne(ac => ac.Rule)
          .WithMany(am => am.RuleTargetOverride)
          .HasForeignKey(ac => ac.RuleId)
          .OnDelete(DeleteBehavior.NoAction);
        }
    }
}