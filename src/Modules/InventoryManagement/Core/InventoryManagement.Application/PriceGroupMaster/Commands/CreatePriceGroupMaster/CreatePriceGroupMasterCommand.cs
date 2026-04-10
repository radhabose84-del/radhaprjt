using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster
{
    public class CreatePriceGroupMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string PriceGroupCode { get; set; } = string.Empty;
        public string PriceGroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
    }
}
