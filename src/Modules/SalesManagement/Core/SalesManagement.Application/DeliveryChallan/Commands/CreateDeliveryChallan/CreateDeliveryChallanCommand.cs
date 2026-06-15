using Contracts.Common;
using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan
{
    public class CreateDeliveryChallanCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public DateOnly DeliveryDate { get; set; }
        public int StoHeaderId { get; set; }
        public int FromPlantId { get; set; }
        public int FromStorageLocationId { get; set; }
        public int ToPlantId { get; set; }
        public int ToStorageLocationId { get; set; }
        public int TransporterId { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal? TransportDistance { get; set; }
        public decimal ConsignmentValue { get; set; }

        // FK → Sales.MiscMaster (MiscType = "TransMode": ByRoad / ByAir). Optional.
        public int? TransportModeId { get; set; }

        // FK → Sales.MiscMaster (MiscType = "DCType": Returnable / Non-Returnable)
        public int DcTypeId { get; set; }

        // FK → Sales.MovementTypeConfig
        public int MovementTypeId { get; set; }

        public string? Remarks { get; set; }
        public List<CreateDeliveryChallanDetailDto>? Details { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
