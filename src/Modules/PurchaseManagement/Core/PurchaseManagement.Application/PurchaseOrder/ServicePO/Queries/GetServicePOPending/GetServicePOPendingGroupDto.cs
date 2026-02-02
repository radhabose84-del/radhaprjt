using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending
{
    public class GetServicePOPendingGroupDto
    {
    public int Id { get; set; }
    public string PONumber { get; set; } = "";
    public DateTimeOffset PODate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorMobile { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal PandFTotal { get; set; }
    public decimal MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal CGSTTotal { get; set; }
    public decimal SGSTTotal { get; set; }
    public decimal IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public decimal InsuranceTotal { get; set; }
    public decimal TDSTotal { get; set; }
    public decimal AdvanceAmount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string CreatedByName { get; set; } = "";
    public string StatusCode { get; set; } = "";
    public string POCategoryCode { get; set; } = "";
    public string POMethodCode { get; set; } = "";

    // Service header bits
    public int ServicePoHeaderId { get; set; }
    public int ServiceCategoryId { get; set; }
    public int ContractTypeId { get; set; }
    public int FrequencyId { get; set; }
    public DateTimeOffset? ValidityFrom { get; set; }
    public DateTimeOffset? ValidityTo { get; set; }
    public int TotalOccurrences { get; set; }
    public decimal OverallLimit { get; set; }
    public string? TermDescription { get; set; }
    public string? POImage { get; set; }
    public string? BillingAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public int? CostCenterId { get; set; }
    public decimal? FreightCharges { get; set; }
    public int? ModeOfDispatchId { get; set; }
    public int? TermsId { get; set; }
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; } 
    public byte IsEdit { get; set; }

    public List<GetPOServicePendingDto> Lines { get; set; } = new();
    }
}