using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster
{
    public class CreateDiscountMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
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

        // Child collections
        public List<DiscountSlabItem>? Slabs { get; set; }
        public List<int>? SalesGroupIds { get; set; }
        public List<int>? PaymentTermIds { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    public class DiscountSlabItem
    {
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }
    }
}
