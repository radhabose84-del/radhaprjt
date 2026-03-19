using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.CreateUsageType
{
    public class CreateUsageTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? UsageTypeCode { get; set; }
        public string? UsageTypeName { get; set; }
        public string? Description { get; set; }
    }
}
