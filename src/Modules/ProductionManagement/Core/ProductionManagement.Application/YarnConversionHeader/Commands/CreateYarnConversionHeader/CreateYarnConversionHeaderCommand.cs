using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader
{
    public class CreateYarnConversionHeaderCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly ConversionDate { get; set; }
        public int LotId { get; set; }

        // Source (Old)
        public int OldItemId { get; set; }
        public int OldPackTypeId { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public int OldBinId { get; set; }
        public int? FaultId { get; set; }

        // Target (New)
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseQty { get; set; }
        public int? LooseHandlingId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Waste
        public int? WasteTypeId { get; set; }
        public decimal WasteQty { get; set; }
        public string? WasteReason { get; set; }

        // Other
        public string? Remarks { get; set; }
    }
}
