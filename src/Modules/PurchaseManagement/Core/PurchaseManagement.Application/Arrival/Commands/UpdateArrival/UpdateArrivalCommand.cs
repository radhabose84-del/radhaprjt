using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;

namespace PurchaseManagement.Application.Arrival.Commands.UpdateArrival
{
    public class UpdateArrivalCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }
        public int RawMaterialPOId { get; set; }
        public string VehicleNumber { get; set; } = default!;

        public int SupplierId { get; set; }
        public int StationId { get; set; }
        public int GodownId { get; set; }
        public int TransporterId { get; set; }
        public int? VmrId { get; set; }            // Gate.VehicleMovementRecord
        public string? SupplierLotNo { get; set; }

        public decimal? FreightRate { get; set; }
        public string? InvoiceGstNo { get; set; }
        public string? LrNumber { get; set; }
        public string? ContainerNo { get; set; }

        public DateTimeOffset? LorryIn { get; set; }
        public DateTimeOffset? LorryOut { get; set; }

        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal PartyWeight { get; set; }
        public decimal WeightDifference { get; set; }
        public decimal? MoisturePercentage { get; set; }

        // PR range (from–to) — optional
        public int? PRFrom { get; set; }
        public int? PRTo { get; set; }

        public int QcStatusId { get; set; }

        public string? Remarks { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public List<UpdateArrivalDetailDto> Details { get; set; } = new();
    }

    public class UpdateArrivalDetailDto
    {
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public int PackTypeId { get; set; }
        public int MixCodeId { get; set; }
        public int UomId { get; set; }
        public decimal Rate { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal ArrivedQty { get; set; }
        public decimal CancelledQty { get; set; }

        public string? BatchNumber { get; set; }
        public long BaleNumberFrom { get; set; }
        public long BaleNumberTo { get; set; }
        public int TotalBaleCount { get; set; }

        public List<ArrivalBaleDto> BaleDetails { get; set; } = new();
    }
}
