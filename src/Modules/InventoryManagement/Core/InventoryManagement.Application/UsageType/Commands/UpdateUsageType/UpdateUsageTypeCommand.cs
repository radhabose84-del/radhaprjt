using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.UpdateUsageType
{
    public class UpdateUsageTypeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? UsageTypeName { get; set; }
        public string? Description { get; set; }
        public int ModuleId { get; set; }
        public int IsActive { get; set; }
    }
}
