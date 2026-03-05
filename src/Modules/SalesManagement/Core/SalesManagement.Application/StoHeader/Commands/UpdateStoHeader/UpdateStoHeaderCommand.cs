using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoHeader.Commands.UpdateStoHeader
{
    public class UpdateStoHeaderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateOnly DocumentDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }
        public int StoTypeId { get; set; }
        public int MovementTypeId { get; set; }
        public int SupplyingPlantId { get; set; }
        public int SupplyingStorageLocationId { get; set; }
        public int ReceivingPlantId { get; set; }
        public int ReceivingStorageLocationId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<UpdateStoDetailDto>? StoDetails { get; set; }
    }

    public class UpdateStoDetailDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }
        public decimal TransferPrice { get; set; }
    }
}
