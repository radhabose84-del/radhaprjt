using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class AgentCustomerMappingConfiguration : IEntityTypeConfiguration<AgentCustomerMapping>
    {
        public void Configure(EntityTypeBuilder<AgentCustomerMapping> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("AgentCustomerMapping", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CustomerId)
                .HasColumnName("CustomerId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentId)
                .HasColumnName("AgentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SubAgentId)
                .HasColumnName("SubAgentId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.SalesGroupId)
                .HasColumnName("SalesGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EffectiveFrom)
                .HasColumnName("EffectiveFrom")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(t => t.EffectiveTo)
                .HasColumnName("EffectiveTo")
                .HasColumnType("datetime")
                .IsRequired(false);

            builder.Property(t => t.IsDefaultAgent)
                .HasColumnName("IsDefaultAgent")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.CustomerId).HasDatabaseName("IX_AgentCustomerMapping_CustomerId");
            builder.HasIndex(t => t.AgentId).HasDatabaseName("IX_AgentCustomerMapping_AgentId");
            builder.HasIndex(t => t.SubAgentId).HasDatabaseName("IX_AgentCustomerMapping_SubAgentId");
            builder.HasIndex(t => t.SalesGroupId).HasDatabaseName("IX_AgentCustomerMapping_SalesGroupId");
            builder.HasIndex(t => new { t.CustomerId, t.IsDefaultAgent })
                .HasDatabaseName("IX_AgentCustomerMapping_CustomerId_IsDefaultAgent");

            // Cross-module FKs (CustomerId, AgentId, SubAgentId) — NO DB FK constraints

            // Same-module FK — DB FK constraint with Restrict
            builder.HasOne(t => t.SalesGroup)
                .WithMany()
                .HasForeignKey(t => t.SalesGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
