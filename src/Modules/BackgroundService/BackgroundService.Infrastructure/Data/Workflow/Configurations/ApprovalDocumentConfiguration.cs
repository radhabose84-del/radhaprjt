using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackgroundService.Infrastructure.Data.Workflow.Configurations
{
    public class ApprovalDocumentConfiguration : IEntityTypeConfiguration<ApprovalDocument>
    {
        public void Configure(EntityTypeBuilder<ApprovalDocument> builder)
        {
            builder.ToTable("ApprovalDocument", "AppData");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ApprovalRequestId)
            .HasColumnName("ApprovalRequestId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.FileName)
            .HasColumnName("FileName")
            .HasColumnType("nvarchar(255)")
            .IsRequired();

            builder.Property(t => t.FilePath)
            .HasColumnName("FilePath")
            .HasColumnType("nvarchar(500)")
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
                
                builder.HasOne(ac => ac.ApprovalRequest)
                .WithMany(am => am.ApprovalDocuments)
                .HasForeignKey(ac => ac.ApprovalRequestId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}