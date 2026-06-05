using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO
{
    public class UpdateRawMaterialPOCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateTimeOffset PODate { get; set; }
        public int ProcurementDocumentTypeId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        // Header totals — saved as supplied in the payload
        public decimal? TaxableTotal { get; set; }
        public decimal? TotalGstAmount { get; set; }
        public decimal? NetTotal { get; set; }

        public List<UpdateRawMaterialPODetailDto> Details { get; set; } = new();
    }

    public class UpdateRawMaterialPODetailDto
    {
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal Rate { get; set; }

        // GST / value fields — saved as supplied in the payload
        public decimal ItemValue { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public decimal? CGSTValue { get; set; }
        public decimal? SGSTValue { get; set; }
        public decimal? IGSTValue { get; set; }
        public decimal TotalGST { get; set; }
        public decimal NetValue { get; set; }
    }
}
