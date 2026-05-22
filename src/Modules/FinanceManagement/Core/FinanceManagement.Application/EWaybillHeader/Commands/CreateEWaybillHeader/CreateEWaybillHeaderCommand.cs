using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader
{
    public class CreateEWaybillHeaderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int? EInvoiceHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? EWBNumber { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public decimal InvoiceValue { get; set; }
        public string? SupplyType { get; set; }
        public string? SubSupplyType { get; set; }
        public string? DocumentType { get; set; }
        public int? TransactionType { get; set; }
        public string? FromGSTIN { get; set; }
        public string? FromTradeName { get; set; }
        public string? ToGSTIN { get; set; }
        public string? ToTradeName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterGSTIN { get; set; }
        public string? TransporterName { get; set; }
        public string? TransportMode { get; set; }
        public string? TransDocNo { get; set; }
        public DateOnly? TransDocDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? VehicleType { get; set; }
        public int? Distance { get; set; }
        public int? PartyId { get; set; }
        public string? EwbStatus { get; set; }

        /// <summary>
        /// Optional line items. Empty list = header-only (original behaviour).
        /// Each entry becomes one Finance.EWaybillDetail row inserted in the same save.
        /// </summary>
        public List<CreateEWaybillDetailDto> Details { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
