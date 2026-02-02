using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class IndentLogConfiguration : IEntityTypeConfiguration<IndentLog>
    {
        public void Configure(EntityTypeBuilder<IndentLog> builder)
        {
          
            
              builder.ToTable("IndentLog", "Purchase");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();
            

            builder.Property(m => m.IndentHeaderId) 
                .HasColumnName("IndentHeaderId")
                .HasColumnType("int")  
                .IsRequired();

            builder.Property(m => m.ActionType)
                   .HasColumnName("ActionType")
                   .HasColumnType("varchar(50)")  
                   .IsRequired();


            builder.Property(m => m.ActionRemarks)
                   .HasColumnName("ActionRemarks")
                   .HasColumnType("varchar(max)")
                   .IsRequired();

            builder.Property(m => m.PreviousData)
                   .HasColumnName("PreviousData")
                   .HasColumnType("nvarchar(max)")
                   .IsRequired(false);

                    builder.Property(m => m.NewData) 
                .HasColumnName("NewData")
                .HasColumnType("nvarchar(max)")  
                .IsRequired(false);

                 builder.Property(m => m.StatusId) 
                .HasColumnName("StatusId")
                .HasColumnType("int")  
                .IsRequired();


            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

        }
    }
}