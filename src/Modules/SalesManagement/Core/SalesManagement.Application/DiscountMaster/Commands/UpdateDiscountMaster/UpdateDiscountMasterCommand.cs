using Contracts.Common;
using MediatR;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;

namespace SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster
{
    public class UpdateDiscountMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? DiscountName { get; set; }
        public int TriggerEventId { get; set; }
        public int DiscountBasisId { get; set; }
        public int ExecutionTypeId { get; set; }
        public int? CurrencyId { get; set; }
        public int? CustomerGroupId { get; set; }
        public int Priority { get; set; }
        public bool RequiresApproval { get; set; }
        public int? MaxDiscountLimitTypeId { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public bool IsStackable { get; set; }
        public int? ExclusionGroupId { get; set; }
        public int ValueTypeId { get; set; }
        public int SlabTypeId { get; set; }
        public int IsActive { get; set; }

        // Child collections (replace strategy)
        public List<DiscountSlabItem>? Slabs { get; set; }
        public List<int>? SalesGroupIds { get; set; }
        public List<int>? PaymentTermIds { get; set; }
    }
}
