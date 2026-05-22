using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader
{
    public class UpdateEWaybillHeaderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SupplyType { get; set; }
        public string? SubSupplyType { get; set; }
        public string? DocumentType { get; set; }
        public int? TransactionType { get; set; }
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
        public string? CancelReason { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
