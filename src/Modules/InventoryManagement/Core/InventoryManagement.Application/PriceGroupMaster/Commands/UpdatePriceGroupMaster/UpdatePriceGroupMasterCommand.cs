using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster
{
    public class UpdatePriceGroupMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string PriceGroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public int IsActive { get; set; }
    }
}
