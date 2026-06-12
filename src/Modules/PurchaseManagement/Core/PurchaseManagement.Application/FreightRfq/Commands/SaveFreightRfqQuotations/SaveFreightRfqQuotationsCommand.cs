using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations
{
    /// <summary>
    /// Incremental save of transporter quotations ("Save Quotations"). Existing rows are updated by Id,
    /// newly-added transporters (Id = 0) are inserted, and rows omitted from the set are removed.
    /// </summary>
    public class SaveFreightRfqQuotationsCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightRfqId { get; set; }
        public List<FreightRfqQuotationInputDto> Quotations { get; set; } = new();
    }

    public class FreightRfqQuotationInputDto
    {
        public int Id { get; set; }                         // existing quotation row id (0 for a newly added transporter)
        public int TransporterId { get; set; }
        public int? TransportDetailId { get; set; }
        public int? RateBasisId { get; set; }
        public decimal? QuotedRate { get; set; }            // null until the transporter replies
        public int? NoOfVehicles { get; set; }
        public string? VehicleNo { get; set; }
        public string? TransportModeName { get; set; }
        public string? VehicleTypeName { get; set; }
        public string? Remarks { get; set; }

        // Party contact — used to email newly added transporters (rows with Id = 0).
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
    }
}
