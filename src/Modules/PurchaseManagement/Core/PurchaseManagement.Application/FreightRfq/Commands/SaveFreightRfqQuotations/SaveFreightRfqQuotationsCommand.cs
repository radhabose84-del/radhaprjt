using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations
{
    /// <summary>Replaces the transporter quotation rows for a Draft Freight RFQ ("Save Quotations").</summary>
    public class SaveFreightRfqQuotationsCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightRfqId { get; set; }
        public List<FreightRfqQuotationInputDto> Quotations { get; set; } = new();
    }

    public class FreightRfqQuotationInputDto
    {
        public int TransporterId { get; set; }
        public int RateBasisId { get; set; }
        public decimal QuotedRate { get; set; }
        public int? NoOfVehicles { get; set; }
        public string? Remarks { get; set; }
    }
}
