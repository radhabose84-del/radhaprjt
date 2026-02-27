using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class OfficerAgentConfiguration : IEntityTypeConfiguration<OfficerAgent>
    {
        public void Configure(EntityTypeBuilder<OfficerAgent> builder)
        {
            builder.ToTable("OfficerAgent", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentId)
                .HasColumnName("AgentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MarketingOfficerId)
                .HasColumnName("MarketingOfficerId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ValidityFrom)
                .HasColumnName("ValidityFrom")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.ValidityTo)
                .HasColumnName("ValidityTo")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
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
            builder.HasIndex(t => t.AgentId);
            builder.HasIndex(t => t.MarketingOfficerId);
            builder.HasIndex(t => new { t.ValidityFrom, t.ValidityTo });

            // Same-module FK — MarketingOfficer (DB constraint)
            builder.HasOne(t => t.MarketingOfficer)
                .WithMany()
                .HasForeignKey(t => t.MarketingOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FK: AgentId → PartyManagement — NO DB constraint
        }
    }
}
