using Contracts.Common;
using MediatR;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt
{
    public class CreateStoReceiptCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly StoReceiptDate { get; set; }
        public int DeliveryChallanHeaderId { get; set; }
        public int ReceivingPlantId { get; set; }
        public int ReceivingStorageLocationId { get; set; }
        public int? BinId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? Remarks { get; set; }
        public List<CreateStoReceiptDetailDto>? Details { get; set; }
    }
}
