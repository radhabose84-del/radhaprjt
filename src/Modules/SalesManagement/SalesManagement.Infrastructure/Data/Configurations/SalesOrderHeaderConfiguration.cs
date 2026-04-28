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
                .IsRequired(false);

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyAddress)
                .HasColumnName("PartyAddress")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(t => t.AgentId)
                .HasColumnName("AgentId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.SubAgentId)
                .HasColumnName("SubAgentId")
                .HasColumnType("int")
                .IsRequired(false);

            // Sales Order Type (cross-module FK — no DB constraint)
            builder.Property(t => t.SalesOrderTypeId)
                .HasColumnName("SalesOrderTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            // Sales Order Type Master (same-module FK — DB constraint)
            builder.Property(t => t.SalesOrderTypeMasterId)
                .HasColumnName("SalesOrderTypeMasterId")
                .HasColumnType("int")
                .IsRequired(false);

            // Sales Enquiry Header (same-module FK — DB constraint)
            builder.Property(t => t.SalesEnquiryHeaderId)
                .HasColumnName("SalesEnquiryHeaderId")
                .HasColumnType("int")
                .IsRequired(false);

            // Reference numbers (nullable)
            builder.Property(t => t.CustomerPoRefno)
                .HasColumnName("CustomerPoRefno")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.ComplaintRefno)
                .HasColumnName("ComplaintRefno")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            // Order Unit (cross-module FK — no DB constraint)
            builder.Property(t => t.OrderUnitId)
                .HasColumnName("OrderUnitId")
                .HasColumnType("int")
                .IsRequired(false);

            // Commercial Details
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

            // MD Discount
            builder.Property(t => t.IsMdDiscountEnabled)
                .HasColumnName("IsMdDiscountEnabled")
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.MdDiscountPercentage)
                .HasColumnName("MdDiscountPercentage")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.MdDiscountValue)
                .HasColumnName("MdDiscountValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.TotalDiscountValue)
                .HasColumnName("TotalDiscountValue")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.MdDiscountRate)
                .HasColumnName("MdDiscountRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.MdApprovalDocument)
                .HasColumnName("MdApprovalDocument")
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            // Agent Commission
            builder.Property(t => t.AgentCommissionId)
                .HasColumnName("AgentCommissionId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.AgentPaymentTermsId)
                .HasColumnName("AgentPaymentTermsId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AgentCommissionSlabId)
                .HasColumnName("AgentCommissionSlabId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.CommissionRate)
                .HasColumnName("CommissionRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.CommissionValue)
                .HasColumnName("CommissionValue")
                .HasColumnType("decimal(18,3)")
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

            // Revision tracking
            builder.Property(t => t.RevisionNumber)
                .HasColumnName("RevisionNumber")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            // Cancelled fields
            builder.Property(t => t.CancelledDate)
                .HasColumnName("CancelledDate")
                .IsRequired(false);

            builder.Property(t => t.CancelledByName)
                .HasColumnName("CancelledByName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.CancelledIP)
                .HasColumnName("CancelledIP")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            // ForeClosed fields
            builder.Property(t => t.ForeClosedDate)
                .HasColumnName("ForeClosedDate")
                .IsRequired(false);

            builder.Property(t => t.ForeClosedByName)
                .HasColumnName("ForeClosedByName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.ForeClosedIP)
                .HasColumnName("ForeClosedIP")
                .HasColumnType("varchar(50)")
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

            builder.HasOne(t => t.SalesQuotation)
                .WithMany(q => q.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesQuotationHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesOrderHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Agent Commission same-module FK constraints
            builder.HasOne(t => t.AgentCommissionConfig)
                .WithMany(c => c.SalesOrderHeaders)
                .HasForeignKey(t => t.AgentCommissionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // AgentPaymentTermsId is a cross-module lookup — no DB FK constraint

            builder.HasOne(t => t.AgentCommissionSlab)
                .WithMany(s => s.SalesOrderHeaders)
                .HasForeignKey(t => t.AgentCommissionSlabId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales Order Type Master (same-module FK — nullable)
            builder.HasOne(t => t.SalesOrderTypeMaster)
                .WithMany(m => m.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesOrderTypeMasterId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales Enquiry Header (same-module FK — nullable)
            builder.HasOne(t => t.SalesEnquiryHeader)
                .WithMany(e => e.SalesOrderHeaders)
                .HasForeignKey(t => t.SalesEnquiryHeaderId)
                .IsRequired(false)
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
            builder.HasIndex(t => t.SubAgentId);
            builder.HasIndex(t => t.SalesGroupId);
            builder.HasIndex(t => t.OrderDate);
            builder.HasIndex(t => t.SalesQuotationHeaderId);
            builder.HasIndex(t => t.AgentCommissionId);
            builder.HasIndex(t => t.AgentPaymentTermsId);
            builder.HasIndex(t => t.AgentCommissionSlabId);
            builder.HasIndex(t => t.SalesOrderTypeMasterId);
            builder.HasIndex(t => t.SalesEnquiryHeaderId);
        }
    }
}
