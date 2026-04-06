using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster
{
    public class CreateDiscountMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? DiscountName { get; set; }
        public int DiscountTypeId { get; set; }
        public int ApplicableLevelId { get; set; }
        public int TriggerEventId { get; set; }
        public bool RequiresApproval { get; set; }
        public int? MaxDiscountLimitTypeId { get; set; }
        public int ValueTypeId { get; set; }
        public decimal? DiscountValue { get; set; }
        public int? SlabTypeId { get; set; }

        // Child collections
        public List<DiscountSlabItem>? Slabs { get; set; }
        public List<int>? SalesGroupIds { get; set; }
        public List<int>? PaymentTermIds { get; set; }
    }

    public class DiscountSlabItem
    {
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
