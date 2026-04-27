using Contracts.Common;
using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice
{
    public class CreateDispatchAdviceCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly DispatchDate { get; set; }
        public int SalesOrderId { get; set; }
        public int PartyId { get; set; }
        public decimal TotOrderQty { get; set; }
        public decimal TotDispatchedQty { get; set; }
        public decimal TotPendingQty { get; set; }
        public int? DispatchAddressId { get; set; }
        public int DispatchTypeId { get; set; }
        public int FreightId { get; set; }
        public int? TransporterId { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? LRNo { get; set; }
        public int UnitId { get; set; }
        public decimal Distance { get; set; }
        public List<CreateDispatchAdviceDetailDto>? Details { get; set; }
    }
}
