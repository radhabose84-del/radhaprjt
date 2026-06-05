using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO
{
    public class CreateRawMaterialPOCommand : IRequest<ApiResponseDTO<int>>
    {
        // OCR Reference Number (only Approved OCRs convertible)
        public int OcrId { get; set; }
        public DateTimeOffset PODate { get; set; }

        // Purchase Order / Procurement Agreement (MiscMaster)
        public int ProcurementDocumentTypeId { get; set; }

        public string? Remarks { get; set; }

        // Header totals — saved as supplied in the payload
        public decimal? TaxableTotal { get; set; }
        public decimal? TotalGstAmount { get; set; }
        public decimal? NetTotal { get; set; }

        public List<CreateRawMaterialPODetailDto> Details { get; set; } = new();
    }

    public class CreateRawMaterialPODetailDto
    {
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public decimal Quantity { get; set; }   // Bales to convert
        public decimal? Weight { get; set; }     // Kgs
        public decimal Rate { get; set; }        // Per candy

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
