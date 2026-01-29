using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalRequestLineConfiguration : IEntityTypeConfiguration<ApprovalRequestLine>
    {
        public void Configure(EntityTypeBuilder<ApprovalRequestLine> builder)
        {
            builder.ToTable("ApprovalRequestLine", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ApprovalRequestId)
            .HasColumnName("ApprovalRequestId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.ModuleLineTransactionId)
            .HasColumnName("ModuleLineTransactionId")
            .HasColumnType("int")
            .IsRequired();


            builder.Property(t => t.StatusId)
           .HasColumnName("StatusId")
           .HasColumnType("int")
           .IsRequired();


            builder.Property(t => t.Remark)
            .HasColumnName("Remark")
            .HasColumnType("Varchar(max)")
            .IsRequired(false);

            builder.Property(cf => cf.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(cf => cf.ModifiedIP)
                .HasColumnType("varchar(255)");

            builder.HasOne(ac => ac.ApprovalRequest)
      .WithMany(am => am.ApprovalRequestLines)
      .HasForeignKey(ac => ac.ApprovalRequestId)
      .OnDelete(DeleteBehavior.NoAction);
           
           builder.HasOne(ac => ac.Status)
           .WithMany(am => am.ApprovalRequestLineStatus)
           .HasForeignKey(ac => ac.StatusId)
           .OnDelete(DeleteBehavior.NoAction);
        }
    }
}