using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyActivityLogConfiguration : IEntityTypeConfiguration<PartyActivityLog>
    {
        public void Configure(EntityTypeBuilder<PartyActivityLog> builder)
        {
            builder.ToTable("PartyActivityLog", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(m => m.PartyId)  // Foreign Key column
              .HasColumnName("PartyId")
              .HasColumnType("int")  // Set as int
              .IsRequired();

            builder.Property(m => m.TableName)
                .HasColumnName("TableName")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(m => m.ColumnName)
                .HasColumnName("ColumnName")
                .HasColumnType("nvarchar(100)");

            builder.Property(m => m.OldValue)
                .HasColumnName("OldValue")
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.NewValue)
                .HasColumnName("NewValue")
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.ActionType)
                .HasColumnName("ActionType")
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            builder.Property(m => m.ChangedBy)  
              .HasColumnName("ChangedBy")
              .HasColumnType("int")  
              .IsRequired();

            builder.Property(m => m.ChangedByName)
                .HasColumnName("ChangedByName")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(m => m.ChangedIp)
                .HasColumnName("ChangedIp")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(m => m.ChangedOn)
                .HasColumnName("ChangedOn")
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("GETDATE()");                   

        }
    }
}