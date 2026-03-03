using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder
{
    public class UpdateSalesOrderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int IsActive { get; set; }

        // Quotation Reference (optional)
        public int? SalesQuotationHeaderId { get; set; }

        // Customer & Unit Details
        public int SalesGroupId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int EnquiryType { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }

        // Commercial Details
        public int? DiscountPlanId { get; set; }
        public int PaymentTermsId { get; set; }
        public int? PaymentTypeId { get; set; }
        public int FreightTypeId { get; set; }
        public int? CountListId { get; set; }
        public string? Remarks { get; set; }

        // File Attachments
        public string? VisitNotesAttachment { get; set; }
        public string? AgentPOAttachment { get; set; }

        // Dispatch Location
        public int DispatchLocationType { get; set; }
        public int? DispatchDepotId { get; set; }
        public int? DispatchUnitId { get; set; }

        // Derived Summary Fields
        public int TotalBags { get; set; }
        public decimal TotalWeightKgs { get; set; }
        public decimal TotalDiscountPerKg { get; set; }
        public decimal ItemValue { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalWithGST { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TotalTCS { get; set; }
        public decimal FinalAmount { get; set; }

        // Line Items
        public List<UpdateSalesOrderDetailDto>? SalesOrderDetails { get; set; }
    }
}
