using Contracts.Common;
using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Commands.UpdateDispatchAdvice
{
    public class UpdateDispatchAdviceCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateOnly DispatchDate { get; set; }
        public int SalesOrderId { get; set; }
        public int PartyId { get; set; }
        public decimal TotOrderQty { get; set; }
        public decimal TotDispatchedQty { get; set; }
        public decimal TotPendingQty { get; set; }
        public int DispatchAddressId { get; set; }
        public int? TransporterId { get; set; }
        public string? VehicleNo { get; set; }
        public string? DriverName { get; set; }
        public string? LRNo { get; set; }
        public int IsActive { get; set; }
        public List<CreateDispatchAdviceDetailDto>? Details { get; set; }
    }
}
