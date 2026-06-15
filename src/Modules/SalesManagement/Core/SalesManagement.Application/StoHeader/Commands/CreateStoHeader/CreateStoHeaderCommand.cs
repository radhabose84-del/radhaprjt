using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoHeader.Commands.CreateStoHeader
{
    public class CreateStoHeaderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public DateOnly DocumentDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }
        public int StoTypeId { get; set; }
        public int MovementTypeId { get; set; }
        public int SupplyingPlantId { get; set; }
        public int SupplyingStorageLocationId { get; set; }
        public int ReceivingPlantId { get; set; }
        public int ReceivingStorageLocationId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateStoDetailDto>? StoDetails { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    public class CreateStoDetailDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }
        public decimal TransferPrice { get; set; }
    }
}
