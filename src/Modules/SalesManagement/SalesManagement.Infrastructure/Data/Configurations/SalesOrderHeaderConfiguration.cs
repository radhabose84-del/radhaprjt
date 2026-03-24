using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderHeaderConfiguration : IEntityTypeConfiguration<SalesOrderHeader>
    {
        public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesOrderHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderNo)
                .HasColumnName("SalesOrderNo")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.OrderDate)
                .HasColumnName("OrderDate")
                .HasColumnType("date")
                .IsRequired();

            // Customer & Unit Details
            builder.Property(t => t.SalesGroupId)
                .HasColumnName("SalesGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesSegmentId)
                .HasColumnName("SalesSegmentId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.EnquiryType)
                .HasColumnName("EnquiryType")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentId)
                .HasColumnName("AgentId")
                .HasColumnType("int")
                .IsRequired(false);

            // Commercial Details
            builder.Property(t => t.DiscountPlanId)
                .HasColumnName("DiscountPlanId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.PaymentTermsId)
                .HasColumnName("PaymentTermsId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PaymentTypeId)
                .HasColumnName("PaymentTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.FreightTypeId)
                .HasColumnName("FreightTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CountListId)
                .HasColumnName("CountListId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            // File Attachments
            builder.Property(t => t.VisitNotesAttachment)
                .HasColumnName("VisitNotesAttachment")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(t => t.AgentPOAttachment)
                .HasColumnName("AgentPOAttachment")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            // Dispatch Location
            builder.Property(t => t.DispatchLocationType)
                .HasColumnName("DispatchLocationType")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchDepotId)
                .HasColumnName("DispatchDepotId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.DispatchUnitId)
                .HasColumnName("DispatchUnitId")
                .HasColumnType("int")
                .IsRequired(false);

            // Derived Summary Fields
            builder.Property(t => t.TotalBags)
                .HasColumnName("TotalBags")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalWeightKgs)
                .HasColumnName("TotalWeightKgs")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalDiscountPerKg)
                .HasColumnName("TotalDiscountPerKg")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ItemValue)
                .HasColumnName("ItemValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalFreight)
                .HasColumnName("TotalFreight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TaxableAmount)
                .HasColumnName("TaxableAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.GSTPercentage)
                .HasColumnName("GSTPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalGST)
                .HasColumnName("TotalGST")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalWithGST)
                .HasColumnName("TotalWithGST")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TCSPercentage)
                .HasColumnName("TCSPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalTCS)
                .HasColumnName("TotalTCS")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.FinalAmount)
                .HasColumnName("FinalAmount")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            // Status & Audit
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Quotation Reference
            builder.Property(t => t.SalesQuotationHeaderId)
                .HasColumnName("SalesQuotationHeaderId")
                .HasColumnType("int")
                .IsRequired(false);

            // Approval Status (same-module FK to MiscMaster)
            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

            // Same-module FK constraints
            builder.HasOne(t => t.SalesGroup)
                .WithMany(g => g.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SalesSegment)
                .WithMany(s => s.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesSegmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DiscountPlan)
                .WithMany(m => m.SalesOrderHeadersAsDiscountPlan)
                .HasForeignKey(t => t.DiscountPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.PaymentType)
                .WithMany(m => m.SalesOrderHeadersAsPaymentType)
                .HasForeignKey(t => t.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.FreightType)
                .WithMany(m => m.SalesOrderHeadersAsFreightType)
                .HasForeignKey(t => t.FreightTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CountList)
                .WithMany(m => m.SalesOrderHeadersAsCountList)
                .HasForeignKey(t => t.CountListId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.EnquiryTypeMisc)
                .WithMany(m => m.SalesOrderHeadersAsEnquiryType)
                .HasForeignKey(t => t.EnquiryType)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DispatchLocationTypeMisc)
                .WithMany(m => m.SalesOrderHeadersAsDispatchLocationType)
                .HasForeignKey(t => t.DispatchLocationType)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SalesQuotation)
                .WithMany(q => q.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesQuotationHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesOrderHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection — reverse navigation (Header → Details)
            builder.HasMany(t => t.SalesOrderDetails)
                .WithOne(d => d.SalesOrderHeader)
                .HasForeignKey(d => d.SalesOrderHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.SalesOrderNo).IsUnique();
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.AgentId);
            builder.HasIndex(t => t.SalesGroupId);
            builder.HasIndex(t => t.OrderDate);
            builder.HasIndex(t => t.SalesQuotationHeaderId);
        }
    }
}
