using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesLeadConfiguration : IEntityTypeConfiguration<SalesLead>
    {
        public void Configure(EntityTypeBuilder<SalesLead> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesLead", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.LeadNo)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.HasIndex(t => t.LeadNo)
                .IsUnique()
                .HasFilter("[LeadNo] IS NOT NULL");

            // Party identification
            builder.Property(t => t.PartyId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ProspectCompanyName)
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(t => t.CityId)
                .HasColumnType("int")
                .IsRequired(false);

            // Contact person
            builder.Property(t => t.ContactName)
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.MobileNumber)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.EmailId)
                .HasColumnType("varchar(150)")
                .IsRequired(false);

            builder.Property(t => t.ContactId)
                .HasColumnType("int")
                .IsRequired(false);

            // Requirement snapshot
            builder.Property(t => t.ItemId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.VariantId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.UomId)
                .HasColumnName("UomId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.RequirementQty)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(t => t.ExpectedDate)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            // Ownership & source
            builder.Property(t => t.LeadSourceId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.MarketingOfficerId)
                .HasColumnName("MarketingOfficerId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.InteractionDate)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            // Closure (Close Lead)
            builder.Property(t => t.ClosureTypeId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ClosureReasonId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ConvertWonLeadToId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ClosureRemarks)
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            builder.Property(t => t.ClosureDate)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // Status
            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.PartyId)
                .HasDatabaseName("IX_SalesLead_PartyId");

            builder.HasIndex(t => t.MarketingOfficerId)
                .HasDatabaseName("IX_SalesLead_MarketingPersonId");

            builder.HasIndex(t => t.InteractionDate)
                .HasDatabaseName("IX_SalesLead_InteractionDate");

            builder.HasIndex(t => t.MobileNumber)
                .HasDatabaseName("IX_SalesLead_MobileNumber");

            builder.HasIndex(t => t.UomId)
                .HasDatabaseName("IX_SalesLead_UomId")
                .HasFilter("[UomId] IS NOT NULL");

            builder.HasIndex(t => t.ClosureTypeId)
                .HasDatabaseName("IX_SalesLead_ClosureTypeId")
                .HasFilter("[ClosureTypeId] IS NOT NULL");

            // Same-module FK: ContactId → Sales.SalesContact
            // No navigation collection on SalesContact — relationship is one-directional
            builder.HasOne(t => t.Contact)
                .WithMany()
                .HasForeignKey(t => t.ContactId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: LeadSourceId → Sales.MiscMaster
            // No navigation collection on MiscMaster — relationship is one-directional
            builder.HasOne(t => t.LeadSource)
                .WithMany()
                .HasForeignKey(t => t.LeadSourceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: MarketingOfficerId → Sales.MarketingOfficer
            builder.HasOne(t => t.MarketingOfficer)
                .WithMany()
                .HasForeignKey(t => t.MarketingOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: ClosureTypeId → Sales.MiscMaster
            builder.HasOne(t => t.ClosureType)
                .WithMany()
                .HasForeignKey(t => t.ClosureTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: ClosureReasonId → Sales.MiscMaster
            builder.HasOne(t => t.ClosureReason)
                .WithMany()
                .HasForeignKey(t => t.ClosureReasonId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: ConvertWonLeadToId → Sales.MiscMaster
            builder.HasOne(t => t.ConvertWonLeadTo)
                .WithMany()
                .HasForeignKey(t => t.ConvertWonLeadToId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cross-module FKs (PartyId, CityId, ItemId) — no DB constraint
        }
    }
}
