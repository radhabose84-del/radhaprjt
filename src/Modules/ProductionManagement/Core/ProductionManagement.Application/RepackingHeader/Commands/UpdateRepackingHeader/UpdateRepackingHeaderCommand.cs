using Contracts.Common;
using MediatR;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;

namespace ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader
{
    public class UpdateRepackingHeaderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public DateOnly RepackDate { get; set; }

        // Target (New)
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Source (Old) — header-level
        public int OldItemId { get; set; }
        public int OldPackTypeId { get; set; }

        // Loose
        public decimal LooseConeKgs { get; set; }
        public int? LooseHandlingId { get; set; }

        // Waste
        public int? FaultId { get; set; }
        public int? WasteTypeId { get; set; }
        public decimal WasteQuantity { get; set; }
        public string? WasteReason { get; set; }

        // Other
        public string? Remarks { get; set; }
        public int? LotId { get; set; }
        public int IsActive { get; set; }

        // Details
        public List<CreateRepackingDetailItem> Details { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
