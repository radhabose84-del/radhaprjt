using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq
{
    public class CreateFreightRfqCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateTimeOffset RfqDate { get; set; }
        public DateTimeOffset? RfqValidTill { get; set; }   // optional RFQ expiry
        public int RfqTypeId { get; set; }
        public int? PoReferenceId { get; set; }      // Raw Material PO — required when type = PO Based (validator)
        public int? SupplierId { get; set; }         // from PO -> OCR
        public string? SourceLocation { get; set; }
        public string? SourceStation { get; set; }
        public string? DestinationLocation { get; set; }    // user input
        public string? DestinationStation { get; set; }     // user input
        public decimal TotalQuantity { get; set; }
        public int TotalBaleCount { get; set; }

        // Transporters selected (multi-select from Party); each becomes a quotation row (rate entered later).
        public List<FreightRfqTransporterInputDto> Transporters { get; set; } = new();
    }

    public class FreightRfqTransporterInputDto
    {
        public int TransporterId { get; set; }              // Party id
        public int? TransportDetailId { get; set; }         // Party transportDetails.id
        public int? RateBasisId { get; set; }               // optional pre-fill from transport detail default
        public string? VehicleNo { get; set; }
        public string? TransportModeName { get; set; }
        public string? VehicleTypeName { get; set; }

        // Party contact (from PartyMaster) — used to email the RFQ to the transporter on save.
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
    }
}
