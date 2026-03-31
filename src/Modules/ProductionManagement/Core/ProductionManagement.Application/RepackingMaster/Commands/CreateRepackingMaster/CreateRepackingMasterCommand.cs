using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster
{
    public class CreateRepackingMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly RepackDate { get; set; }
        public int ItemId { get; set; }
        public int? SelectionModeId { get; set; }

        // Source (Old)
        public int OldPackTypeId { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public int OldBinId { get; set; }

        // Target (New)
        public int PackTypeId { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Loose
        public decimal LooseConeKgs { get; set; }
        public int? LooseHandlingId { get; set; }

        // Other
        public string? Remarks { get; set; }
    }
}
